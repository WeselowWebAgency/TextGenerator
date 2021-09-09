from generateModelParams  import GenerateModelParams

def SetParams(length=100 , temperature=1.0, k=10, p=0.9, repetition_penalty=1.0,num_return_sequences=1):
    
    params2 = GenerateModelParams(length, temperature, k, p, repetition_penalty,num_return_sequences)       
    print(params2.max_length)
    return params2

SetParams()
