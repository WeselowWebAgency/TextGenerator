import argparse
import jsonlines
import json
import os
import random
import re
import string  
from copy import deepcopy
from pathlib import Path
import numpy as np
import pandas as pd
import pymorphy2
import spacy
import xx_ent_wiki_sm
from deeppavlov import build_model, configs
from generateModelParams  import GenerateModelParams

morph = pymorphy2.MorphAnalyzer()

nlp = xx_ent_wiki_sm.load()
nlp.add_pipe(nlp.create_pipe('sentencizer'), first=True)

syntax_model = build_model(configs.syntax.syntax_ru_syntagrus_bert, download=True)

from transformers import GPT2LMHeadModel,GPT2Tokenizer
import torch

tokenizer = GPT2Tokenizer.from_pretrained("sberbank-ai/rugpt3large_based_on_gpt2")

model = GPT2LMHeadModel.from_pretrained("sberbank-ai/rugpt3large_based_on_gpt2")
if torch.cuda.is_available():
  device = "cuda"
else:
  device = "cpu"
model.to(device)
#print("Success!")

russian_restricted_pronouns = "я мной меня мною мне мы нас нам нами ты тебя тебе тобою тобой вы вас вам вами".split()
extra_marks = re.compile(r"&[a-zA-Z0-9;]+")
expanding_startings = [
    "В то же время", 
    "Это так", 
    "Действительно,", 
    "Потому что", 
    "А главное,", 
    "При этом",
    "В связи с этим",
    "Кроме того,",
    "Интересно, что",
    "Также",
    ]

def join_words_in_or_pattern(words):
    return "(" + "|".join([r'\b%s\b' % word for word in words]) + ")"

RU_PRONOUNS = re.compile(join_words_in_or_pattern(russian_restricted_pronouns), re.IGNORECASE)

def generate_rugpt3large(prompt_text, return_only_predicted=False,
                         till_new_string=True, generate_only_one_sent=True, 
                         length=100, temperature=1.0, k=10, p=0.9, repetition_penalty=1.0,
                         num_return_sequences=1, stop_token="</s>"):
    global device # cpu
    total_sequence = ""
    if num_return_sequences > 1:
        all_sequences = []
    
    encoded_prompt = tokenizer.encode(
        prompt_text, add_special_tokens=False, return_tensors="pt")
    encoded_prompt = encoded_prompt.to(device)

#output_sequences = model.generate(
                #input_ids=encoded_prompt,
                #max_length=length + len(encoded_prompt[0]),
                #temperature=temperature,
                #top_k=k,
                #top_p=p,
                #repetition_penalty=repetition_penalty,
                #do_sample=True,
                #num_return_sequences=num_return_sequences,
            #)
    
    print(params.max_length)
    print(params.temperature)
    print(params.top_k)
    print(params.top_p)
    print(params.repetition_penalty)
    print(params.do_sample)
    print(params.num_return_sequences)
    
    output_sequences = model.generate(
                input_ids=encoded_prompt,
                max_length=params.max_length + len(encoded_prompt[0]),
                temperature=params.temperature,
                top_k=params.top_k,
                top_p=params.top_p,
                repetition_penalty=params.repetition_penalty,
                do_sample=params.do_sample,
                num_return_sequences=params.num_return_sequences,
            )
    
    if len(output_sequences.shape) > 2:
        output_sequences.squeeze_()

    for generated_sequence_idx, generated_sequence in enumerate(output_sequences):
        generated_sequence = generated_sequence.tolist()
        
         # декодирует из generated_sequence(там находятся числа) в слова
        text = tokenizer.decode(generated_sequence, clean_up_tokenization_spaces=True)
        text = text[: text.find(stop_token) if stop_token else None]

        # записывает в переменную текст под нидексом [len(tokenizer.decode())] и удаляет отсупы
        total_sequence = (text[len(tokenizer.decode(
            encoded_prompt[0], clean_up_tokenization_spaces=True)) :]) 
        total_sequence = re.sub(r"[\"'«»]", "", total_sequence) # записывает только то что есть до первого отступов, если текст в ковычках то записыввает то что в ковчках до резделителя
        total_sequence = total_sequence.strip() # удаляет пробелы и отступы
        
        if till_new_string:
            
            # записывает всё до первой точки или воскл.знака и тд
            total_sequence = total_sequence[:total_sequence.find("\n")].strip() #если в тексте есть  "\n" то удаляет всё то что после него и записывает только всё до окончния предложения
            
        if generate_only_one_sent:
            total_sequence = " ".join([sent.text for sent in nlp(total_sequence).sents][:1])
            
         # условие 1
        if len(total_sequence) > 1 and prompt_text[-1] in [".", "!", "?", "…"]:
            total_sequence = total_sequence[0].upper() + total_sequence[1:]
            
        if not return_only_predicted:
            if len(total_sequence) > 1 and total_sequence[0] in [".", "!", "?", "…", ","]:
                total_sequence = prompt_text + total_sequence
            else:
                total_sequence = prompt_text + " " + total_sequence
        else:
            if len(total_sequence) > 1 and total_sequence[0] in [".", "!", "?", "…", ","]:
                total_sequence = total_sequence
            else:
                total_sequence = " " + total_sequence
        
        if num_return_sequences > 1 and len(total_sequence) > 0:
            all_sequences.append(total_sequence)
            
    if num_return_sequences > 1:
        # вовзращает все сгененированные фразы
        return all_sequences
    else:
        return total_sequence
    

def get_nsubjects(text):
    nsubjects = []
    for parse in syntax_model([text]):
        for row in parse.split("\n"):
            if "nsubj" in row:
                nsubjects.append(row.split("\t")[1])

    return nsubjects


def is_satisfying(sent):
    doc = nlp(sent)
    if len(doc.ents) > 0:
        return False
    
    ntokens = len(sent.split())
    if 15 <= ntokens or ntokens < 5:
        return False
    if re.search(RU_PRONOUNS, sent):
        return False
    
    return True


def generate_paraphrase(text):
    predictions = generate_rugpt3large(f"{text} Перефразирую:", 
                                       return_only_predicted=True, 
                                       num_return_sequences=10)
    predictions = [p.strip() for p in predictions]
    predictions = [p for p in predictions if len(p) > 1]
    original_tokens = set(text.split())
    nsubjects = set(get_nsubjects(text))
    
    total_n_tokens = []
    total_same_tokens = []
    total_same_nsubjects = []
    
    for pred_par in predictions:
        # разбиваем по пробелам
        pred_tokens = pred_par.split()
        
        len_pred_tokens = len(pred_tokens)
        len_same_tokens = len(original_tokens.intersection(set(pred_tokens)))
        len_same_nsubjs = len(nsubjects.intersection(set(pred_tokens)))
        
        total_n_tokens.append(len_pred_tokens)
        total_same_tokens.append(len_same_tokens)
        total_same_nsubjects.append(len_same_nsubjs)
    
    
    targets = [st * sn / tt for st, sn, tt in 
               zip(total_same_tokens, total_same_nsubjects, total_n_tokens)]
    # if all([el <= 1 for el in total_same_nsubjects]):
    if sum(total_same_nsubjects) == 0:
        targets = [st / tt for st, sn, tt in 
                   zip(total_same_tokens, total_same_nsubjects, total_n_tokens)]
    targets = [t if predictions[i] != text else 0 for i, t in enumerate(targets)]
    best_pred_id = np.argmax(targets)
    
    new_sent_text = predictions[best_pred_id]
    if len(new_sent_text) > 1:
        if new_sent_text[-1] not in [".", "!", "?", "…"]:
            new_sent_text = new_sent_text + "."
        new_sent_text = new_sent_text[0].upper() + new_sent_text[1:]
    else:
        new_sent_text = text
    return new_sent_text

    
def expand_text(text, paraphrase=False, max_history_sents=5):
    expanded = deepcopy(text) # возвращает скопированный объект text
    
    doc = nlp(text)
    expanded_sents = []
    sents_texts = [sent.text for sent in doc.sents if len(sent.text) > 1] 
    for sent_id, sent in enumerate(doc.sents):
        if len(sent.text) <= 1:
            continue
        expanded_sents.append(sent.text) # добавлем в массив
        
        expand_sent = ""
        if sent_id < len(sents_texts) - 1 and {"CONJ"} in morph.parse(sents_texts[sent_id + 1].split()[0])[0].tag:
            pass
        else:
            # рандоино выбирает начало фразы
            expanding_start = random.choice(expanding_startings)
            context = " ".join(expanded_sents[-max_history_sents:])
            generated = generate_rugpt3large(f"{context} {expanding_start}",
                                             return_only_predicted=True, num_return_sequences=5)
            
            # формируем предложение
            for gen in generated:
                if is_satisfying(gen):
                    expand_sent = f"{expanding_start}{gen}".strip()
                    if expand_sent[-1] not in [".", "!", "?", "…"]:
                        expand_sent = expand_sent + "."
                    break
        
        new_subtext = deepcopy(sent.text)
        if paraphrase:
            new_sent_text = generate_paraphrase(sent.text).strip()
            if len(new_sent_text) > 1:
                if new_sent_text[-1] not in [".", "!", "?", "…"]: new_sent_text = new_sent_text + "."
                new_sent_text = new_sent_text[0].upper() + new_sent_text[1:]
            else:
                new_sent_text = deepcopy(sent.text)
                
            if len(expand_sent) > 0:
                new_subtext = f"{new_sent_text} {expand_sent}"
                expanded_sents.append(f"{expand_sent}")
            else:
                new_subtext = f"{new_sent_text}"
        else:
            if len(expand_sent) > 0:
                new_subtext = f"{sent.text} {expand_sent}"
                expanded_sents.append(f"{expand_sent}")
                
        expanded = expanded.replace(sent.text, new_subtext)
        
                
    expanded = expanded.strip()
    if abs(len(expanded) - len(text)) < 10:
        expanded = expand_text(text, max_history_sents=5)
    return expanded

def paraphrase_and_expand_text(text, paraphrase=False, expand=False, 
                               max_history_sents=5):
    text = text.strip() 
    if expand:
        # если expand == true
        paraphrased_expanded = expand_text(text, 
                                           paraphrase=paraphrase, 
                                           max_history_sents=max_history_sents)  # генерируем текст
    elif paraphrase:
        paraphrased_expanded = ""
        doc = nlp(text)
        for sent_id, sent in enumerate(doc.sents):
            if len(sent.text) <= 1:
                continue

            if paraphrase:
                par_sent = generate_paraphrase(sent.text)
                paraphrased_expanded += f" {par_sent}"
    paraphrased_expanded = re.sub(extra_marks, "", paraphrased_expanded)
    return paraphrased_expanded.strip()


text = """Многие люди даже не подозревают о том, что вокруг нас есть множество предметов и вещей, которые имеют удивительные свойства. В этой статье обсудим твердость некоторых материалов и интересные результаты, которые получаются на основе этих свойств.
В 1994 году большое землетрясение ударило близ Лос-Анджелеса, убив 57 человек, и поранив более 5 000. Материальный урон достиг невероятных 20 миллиардов долларов. Такие землетрясения заставляют нас задуматься. Насколько твердая земля под нашими ногами? Что вообще значит понятие твердости?
Каменноугольный пек кажется твердым, но это не так. На самом деле он является очень вязкой жидкостью, т.е. он жидкий. Вязкость - это мера сопротивления растеканию. Оливковое масло примерно в 100 раз вязче воды, а мед в 100 раз вязче масла. Вязкость пека больше вязкости воды в 230 миллиардов раз. В Кливлендском университете над пеком проводится самый продолжительный в мире эксперимент. В 1927 году пек был помещен в воронку. За 90 лет из нее упало всего 9 капель. Никто не присутствовал при падении капли. В 1988 году хранитель эксперимента Джон Мейнстон был близок к тому, чтобы увидеть как падает капля. Он вышел из комнаты, чтобы налить себе чаю и пропустил заветный момент. Вы можете наблюдать за этим экспериментом онлайн, но так как последняя капля упала в 2014 году, то вряд ли Вам удастся в ближайшие годы увидеть заветное падение."""


def SetParams(length=100, temperature=1.0, k=10, p=0.9, repetition_penalty=1.0,num_return_sequences=1):
        global params
        params = GenerateModelParams(length, temperature, k, p, repetition_penalty,num_return_sequences)       
        print(params)

def GetArgs():
    parser = argparse.ArgumentParser()
    parser.add_argument("--text", type=str, required=True)
    args = parser.parse_args()
    return args


SetParams(length=100, temperature=1.0, k=10, p=0.9, repetition_penalty=1.0,num_return_sequences=1)

rewritten_text = paraphrase_and_expand_text(text, paraphrase=True, expand=True)
os.system('CLS')
print(rewritten_text)    




    

