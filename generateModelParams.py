class GenerateModelParams:
    
    
    def __init__(self,length, temperature, k, p, repetition_penalty,num_return_sequences) -> None:
        self.max_length=length
        self.temperature=temperature
        self.top_k=k
        self.top_p=p
        self.repetition_penalty = repetition_penalty
        self.do_sample=True
        self.num_return_sequences=num_return_sequences
        pass
    
    def SetParams (length, temperature, k, p, repetition_penalty,num_return_sequences):
                max_length = length
                temperature=temperature,
                top_k=k,
                top_p=p,
                repetition_penalty=repetition_penalty,
                do_sample=True,
                num_return_sequences=num_return_sequences
    
        
    