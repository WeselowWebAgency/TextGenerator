
import os
import sys

from typing import Text
import torch
import random
import argparse
import numpy as np
import clr
clr.AddReference("System.Windows.Forms")






from GPT2.model import (GPT2LMHeadModel) 
from GPT2.utils import load_weight
from GPT2.config import GPT2Config
from GPT2.sample import sample_sequence
from GPT2.encoder import get_encoder




baseDirectory = ""








def text_generator(text, baseDirectory,length = -1 ,temperature = 0.7,top_k = 40, nsamples = 1  ):
    
    state_dict = torch.load(baseDirectory + "gpt2-pytorch_model.bin", map_location='cpu' if not torch.cuda.is_available() else None)
    parser = argparse.ArgumentParser()

    parser.add_argument("--text", type=str, required=False,default=text)
    parser.add_argument("--quiet", type=bool, default=False)
    
    parser.add_argument("--nsamples", type=int, default=nsamples)
    parser.add_argument('--unconditional', action='store_true', help='If true, unconditional generation.')
    
    parser.add_argument("--batch_size", type=int, default=-1)
    parser.add_argument("--length", type=int, default=length)
    parser.add_argument("--temperature", type=float, default=temperature)
    parser.add_argument("--top_k", type=int, default=top_k)
    
    args = parser.parse_args()

    #if args.quiet is False:
        #print(args)

    
    if args.batch_size == -1:
        args.batch_size = 1
    assert args.nsamples % args.batch_size == 0

    seed = random.randint(0, 2147483647)
    np.random.seed(seed)
    torch.random.manual_seed(seed)
    torch.cuda.manual_seed(seed)
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    # Load Model
    enc = get_encoder(baseDirectory + "GPT2\\encoder.json",baseDirectory + "GPT2\\vocab.bpe")
    config = GPT2Config()
    model = GPT2LMHeadModel(config)
    model = load_weight(model, state_dict)
    model.to(device)
    model.eval()

    if args.length == -1:
        args.length = config.n_ctx // 2
    elif args.length > config.n_ctx:
        raise ValueError("Can't get samples longer than window size: %s" % config.n_ctx)

    #print(args.text)
    context_tokens = enc.encode(args.text)

    generated = 0
    for _ in range(args.nsamples // args.batch_size):
        out = sample_sequence(
            model=model, length=args.length,
            context=context_tokens  if not  args.unconditional else None,
            start_token=enc.encoder['<|endoftext|>'] if args.unconditional else None,
            batch_size=args.batch_size,
            temperature=args.temperature, 
            top_k=args.top_k, 
            device=device
        )
        out = out[:, len(context_tokens):].tolist()
        for i in range(args.batch_size):
            generated += 1
            text2 = enc.decode(out[i])
            #if args.quiet is False:
                #print("=" * 40 + " SAMPLE " + str(generated) + " " + "=" * 40)
            
            print(text2)
            return text2
def Get_state_dict():
    state_dict = torch.load(baseDirectory + "\\Gpt test\\тест 2\\gpt2-pytorch_model.bin", map_location='cpu' if not torch.cuda.is_available() else None)
    return state_dict

if __name__ == '__main__':
    if os.path.exists(baseDirectory +"\\Gpt test\\тест 2\\gpt2-pytorch_model.bin"):
        
        Text = '''It was a bright cold day in April, and the clocks were striking thirteen. Winston Smith, his chin nuzzled into his breast in an effort to escape the vile wind, slipped quickly through the glass doors of Victory Mansions, though not quickly enough to prevent a swirl of gritty dust from entering along with him.'''
        text_generator(Text)
    else:
        print('Please download gpt2-pytorch_model.bin')
        sys.exit()
