using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextGenerator.Models;

namespace TextGenerator
{
    public class PythonNetWorker
    {
         
        private readonly string _pathToPythonDirectory;

        public PythonNetWorker(string pathToPythonDirectory, string namePythonDll)
        {
            Runtime.PythonDLL = Path.Combine(pathToPythonDirectory, namePythonDll);
            _pathToPythonDirectory = pathToPythonDirectory;
        }


        /// <summary>
        /// Генерирует текст на английском языке.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textParams"></param>
        /// <returns></returns>
        public string GenerateEngText(string text, TextParams textParams)
        {
            string path = Path.Combine(Path.GetTempPath(), "TextGenerator","Assets","En");

            SetPaths(path);

            using (Py.GIL()) //Initialize the Python engine and acquire the interpreter lock
            {
                try
                {
                    Logger.SaveLog("Запускаем En-генератор ...", LogType.Info);
                    // import your script into the process
                    dynamic sampleModule = Py.Import("main"); // сюда нужно передать название скрипта
                    dynamic results = sampleModule.text_generator(
                        text, 
                        path + "\\", 
                        /*length*/ textParams.Length,
                        /*temperature*/ textParams.Temperature,
                        /*top_k*/  textParams.K,
                        /*nsamples*/ 1); // вызов метода из скрипта
                    Logger.SaveLog("En-генератор выполнил задание успешно ...", LogType.Warning);
                    return results;
                }
                catch (Exception error)
                {
                    // Communicate errors with exceptions from within python script -
                    // this works very nice with pythonnet.
                    Logger.SaveLog($"En-генератор вызвал ошибку - {error.Message} ...", LogType.Error);
                    return null;
                }
            }
        }

        /// <summary>
        /// Генерирует текст на русском языке.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textParams"></param>
        /// <returns></returns>
        public string GenerateRusText(string text, TextParams textParams)
        {
            string path = Path.Combine(Path.GetTempPath(), "TextGenerator", "Assets", "Ru");
            SetPaths(path); // сюда нужно передать путь до папки со скриптом

            using (Py.GIL())
            {
                try
                {
                    Logger.SaveLog("Запускаем Ru-генератор ...", LogType.Info);
                    dynamic sampleModule = Py.Import("text_expansion"); // сюда нужно передать название скрипта
                    dynamic setParams = sampleModule.SetParams(
                        /*length =*/ textParams.Length, 
                        /*temperature =*/ textParams.Temperature,
                        /*k = */  textParams.K,
                        /*p*/ textParams.P,
                        /*repetition_penalty*/ textParams.RepetitionPenalty,
                        /*num_return_sequences*/  textParams.NumReturnSequences); // вызов метода из скрипта
                    dynamic result = sampleModule.paraphrase_and_expand_text(text, textParams.Paraphrase, textParams.Expand); // вызов метода из скрипта
                    Logger.SaveLog("Ru-генератор выполнил задание успешно ...", LogType.Warning);
                    return result;
                }
                catch (PythonException error)
                {
                    // Communicate errors with exceptions from within python script -
                    // this works very nice with pythonnet.
                    Logger.SaveLog($"Ru-генератор вызвал ошибку - {error.Message} ...", LogType.Error);
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
