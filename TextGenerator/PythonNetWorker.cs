using Python.Runtime;
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
        private string _pathToPythonDirectory;
        
        public PythonNetWorker(string pathToPythonDirectory,string namePythonDll)
        {
            Runtime.PythonDLL = pathToPythonDirectory + namePythonDll;
            _namePythonDll = namePythonDll;
            _pathToPythonDirectory = pathToPythonDirectory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="directoryScript"> сюда нужно передать путь до папки в которой хранится скрипт</param>
        /// <returns></returns>
        public string GenerateEngText(string text, string directoryScript)
        {
            SetPaths(directoryScript);

            using (Py.GIL()) //Initialize the Python engine and acquire the interpreter lock
            {
                try
                {
                    // import your script into the process
                    dynamic sampleModule = Py.Import("main"); // сюда нужно передать название скрипта
                    dynamic results = sampleModule.text_generator(text, directoryScript, -1, 0.7,  40,  1); // вызов метода из скрипта
                    return results;
                }
                catch (Exception error)
                {
                    // Communicate errors with exceptions from within python script -
                    // this works very nice with pythonnet.
                    Console.WriteLine("Error occured: ", error.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Генерирует текст на английском языке.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pathPythonFolder"> сюда нужно передать путь до папки в которой хранится скрипт</param>
        /// <returns></returns>
        public string GenerateRusText(string text, string pathPythonFolder) {
            SetPaths(pathPythonFolder); // сюда нужно передать путь до папки со скриптом
            using (Py.GIL()) 
            {
                try
                {
                    dynamic sampleModule = Py.Import("text_expansion"); // сюда нужно передать название скрипта
                    dynamic setParams = sampleModule.SetParams(/*length =*/ 100, /*temperature =*/ 1.0, /*k = */  10, /*p*/  0.9, /*repetition_penalty*/  1.0, /*num_return_sequences*/  1); // вызов метода из скрипта
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

        private void SetPaths(string pathPythonFolder)
        {
            // Setup all paths before initializing Python engine
            string pathToPython = _pathToPythonDirectory;
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
    }
       
}
