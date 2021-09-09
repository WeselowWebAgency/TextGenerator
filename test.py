from generateModelParams  import GenerateModelParams

def start():
    SetParams(length=100 , temperature=1.0, k=10, p=0.9, repetition_penalty=1.0,num_return_sequences=1)
    print(params.max_length)

def SetParams(length=100 , temperature=1.0, k=10, p=0.9, repetition_penalty=1.0,num_return_sequences=1):
    global params
    params2 = GenerateModelParams(length, temperature, k, p, repetition_penalty,num_return_sequences)
    params = params2
    print(params2.max_length)
    return params2






start()