﻿using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextGenerator
{
    public class PythonNetWorker
    {
        private string _namePythonDll;
        private string _pathToPythonDirictory;
        public PythonNetWorker(string pathToPythonDirectory,string namePythonDll)
        {
            Runtime.PythonDLL = pathToPythonDirectory + namePythonDll;
            _namePythonDll = namePythonDll;
            _pathToPythonDirictory = pathToPythonDirectory;
        }


        public string GenerateEngText(string text)
        {

            SetPaths(@"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\Gpt test\тест 2");

            using (Py.GIL()) //Initialize the Python engine and acquire the interpreter lock
            {
                try
                {
                    // import your script into the process
                    dynamic sampleModule = Py.Import("main"); // сюда нужно передать название скрипта
                    dynamic results = sampleModule.text_generator(text); // вызов метода из скрипта
                    return results;
                }
                catch (PythonException error)
                {
                    // Communicate errors with exceptions from within python script -
                    // this works very nice with pythonnet.
                    Console.WriteLine("Error occured: ", error);
                    return null;
                }
            }
        }

        private void SetPaths(string pathPythonFolder) {
            // Setup all paths before initializing Python engine
            string pathToPython = _pathToPythonDirictory;
            string path = pathToPython + ";" +
                          Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", pathToPython, EnvironmentVariableTarget.Process);

            var lib = new[]
            {
                pathPythonFolder,
                Path.Combine(pathToPython, "Lib"),
                Path.Combine(pathToPython, "DLLs")
            };

            string paths = string.Join(";", lib);
            Environment.SetEnvironmentVariable("PYTHONPATH", paths, EnvironmentVariableTarget.Process);

        }


        public string GenerateRusText(string text) {
            SetPaths(@"C:\Users\Admin\Desktop\нейросетка\TextGenerator2"); // сюда нужно передать путь до папки со скриптом
            using (Py.GIL()) 
            {
                try
                {
                    dynamic sampleModule = Py.Import("text_expansion"); // сюда нужно передать название скрипта
                    
                    dynamic result = sampleModule.paraphrase_and_expand_text(text, true, true); // вызов метода из скрипта
                    return result;

                }
                catch (PythonException error)
                {
                    // Communicate errors with exceptions from within python script -
                    // this works very nice with pythonnet.
                    Console.WriteLine("Error occured: ", error);
                    return null;
                }
            }

        }
    }
       
}