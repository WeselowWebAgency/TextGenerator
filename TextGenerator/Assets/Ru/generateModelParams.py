class GenerateModelParams:
    
    
    def __init__(self,length, temperature, k, p, repetition_penalty,num_return_sequences) -> None:
        self.max_length=length
        self.temperature=temperature
        self.top_k=k
        self.top_p=p
        self.repetition_penalty = repetition_penalty
        self.do_sample=True
        self.num_return_sequences=num_return_sequences

        print(self.max_length)
        print(self.temperature)
        print(self.top_k)
        print(self.top_p)
        print(self.repetition_penalty)
        print(self.do_sample)
        print(self.num_return_sequences)
        pass
    
    
    
        
    