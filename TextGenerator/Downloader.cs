using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TextGenerator.Models;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace TextGenerator
{
    public class Downloader
    {

        private readonly WebClient _webClient;
        private readonly string _path;
        private readonly string _pythonPath;
        

        
        public Downloader(string pythonPath)
        {
            _webClient = new WebClient();
            _path = Path.GetTempPath();
            _pythonPath = pythonPath;
           
        }

        public Downloader(IZennoPosterProjectModel project, string pythonPath)
        {
            _pythonPath = pythonPath;
        }

        private string StartProcess(ProcessStartInfo processInfo)
        {
            string result;
            //processInfo.CreateNoWindow = true;
            //processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            using (Process process = Process.Start(processInfo))
            {
                 
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                    Console.Write(result);
                    Thread.Sleep(1000);
                }
            }
            return result;
        }

        public bool CreateDirectories()
        {
            Logger.SaveLog($"Начинаем создание временных папок ...", LogType.Info);

            string baseFolder = _path + "TextGenerator\\";
            if (CreateDirectory(baseFolder) == false) return false;

            string assetsPath = baseFolder + "Assets\\";
            if (CreateDirectory(assetsPath) == false) return false;


            string enScripts = assetsPath + "En\\";
            if (CreateDirectory(enScripts) == false) return false;


            string ruScripts = assetsPath + "Ru\\";
            if (CreateDirectory(ruScripts) == false) return false;


            string gpt2Path = enScripts + "GPT2\\";
            if (CreateDirectory(gpt2Path) == false) return false;

            Logger.SaveLog($"Закончили создание временных папок ...", LogType.Info);

            return true;
        }

        private bool CreateDirectory(string path)
        {
            bool rez = false;

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                rez = true;
            }

            catch (Exception ex)
            {
                Logger.SaveLog($"Ошибка при создании временной папки - {ex.Message}", LogType.Error);
            }

            return rez;
        }


        public bool DownloadPackages()
        {
            //проверяем еще раз наличие папок
            if (!CreateDirectories()) return false;

            Logger.SaveLog("Отсутствуют некоторые зависимости, начинаем скачивание ...", LogType.Info);

            string workingDirectory = "";
            string argument = "";
            string rezultProcess = "";

            //pip
            try
            {
                Logger.SaveLog("Проверка обновления pip ...", LogType.Info);
                workingDirectory = $"{_pythonPath}\\Scripts";
                argument = "install --upgrade pip command";
                rezultProcess = StartProccess(argument, workingDirectory);
                Logger.SaveLog($"Проверка обновления pip закончена. Результат: {rezultProcess}", LogType.Info);
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Ошибка обновления pip - {e.Message}", LogType.Error);
                return false;
            }

            var packages = File.ReadAllLines(Path.Combine(_path, "TextGenerator", "requirements.txt"));

            foreach (var t in packages)
            {
                try
                {
                    Logger.SaveLog($"Проверка зависимости {t}", LogType.Info);
                    argument = $"install {t}";
                    rezultProcess = StartProccess(argument, null);
                    Logger.SaveLog($"Результат проверки зависимости  {t}: {rezultProcess}", LogType.Info);
                }
                catch (Exception e)
                {
                    Logger.SaveLog($"Ошибка проверки зависимости  {t} - {e.Message}", LogType.Error);
                    return false;
                }
            }

            //xx_ent_wiki_sm
            try
            {
                Logger.SaveLog($"Проверка зависимости  xx_ent_wiki_sm ...", LogType.Info);
                argument = $"-m spacy download xx_ent_wiki_sm";
                rezultProcess = StartProccess(argument, null, @"\python.exe");
                Logger.SaveLog($"Результат проверки зависимости xx_ent_wiki_sm: {rezultProcess}", LogType.Info);
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Ошибка проверки зависимости xx_ent_wiki_sm - {e.Message}", LogType.Error);
                return false;
            }

            //squad_bert
            try
            {
                Logger.SaveLog($"Проверка зависимости squad_bert ...", LogType.Info);
                argument = $"-m deeppavlov install squad_bert ";
                rezultProcess = StartProccess(argument, null, @"\python.exe");
                Logger.SaveLog($"Результат проверки зависимости squad_bert: {rezultProcess}", LogType.Info);
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Ошибка проверки зависимости squad_bert - {e.Message}", LogType.Error);
                return false;
            }

            Logger.SaveLog("Скачивание необходимых зависимостей завершено успешно...", LogType.Warning);

            return true;


        }

        public bool DownloadModels()
        {
            string path = Path.Combine(_path, "TextGenerator","Assets","En");

            Logger.SaveLog("Начинаем скачивание модели model_en.bin ... ", LogType.Info);
            var result = DownloadFile("model_en.bin", "https://s3.amazonaws.com/models.huggingface.co/bert/gpt2-pytorch_model.bin", path);
            
            _ = result
                ? Logger.SaveLog("Скачивание модели model_en.bin завершено успешно ...", LogType.Warning)
                : Logger.SaveLog("Ошибка при скачивании модели model_en.bin ...", LogType.Error);

            return result;
        }

        private string StartProccess(string arg, string WorkingDirectory, string fileName = @"\Scripts\pip.exe")
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _pythonPath + fileName;
            start.Arguments = string.Format("{0}", arg);

            start.RedirectStandardInput = true;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WorkingDirectory = WorkingDirectory;
            return StartProcess(start);
        }

        public bool SaveScripts()
        {
            string pathScripts = _path + "TextGenerator\\Assets\\";
            string pathEngScripts = pathScripts + "En\\";
            string gpt2Path = pathEngScripts + "GPT2\\";
            string pathRuScripts = pathScripts + "Ru\\";

            Logger.SaveLog($"Начинаем скачку библиотек для работы с моделями ...", LogType.Info);

            
            if (!DownloadRusScripts(pathRuScripts))
            {
                Logger.SaveLog($"Скачивание библиотеки RuScripts завершено с ошибкой ...", LogType.Info);
                return false;
            }

            if (!DownloadEngScripts(pathEngScripts))
            {
                Logger.SaveLog($"Скачивание библиотеки EngScripts завершено с ошибкой ...", LogType.Info);
                return false;
            }

            if (!DownloadGpt2(gpt2Path))
            {
                Logger.SaveLog($"Скачивание библиотеки GPT завершено с ошибкой ...", LogType.Info);
                return false;
            }

            if (!DownloadFile("requirements.txt",
                "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/requirements.txt",
                _path + "TextGenerator\\")
            )
            {
                Logger.SaveLog($"Скачивание requirements.txt завершено с ошибкой ...", LogType.Info);
                return false;
            }

            Logger.SaveLog($"Скачивание библиотек для работы с моделями завершено успешно ...", LogType.Info);
            return true;
        }

        private bool DownloadRusScripts(string path)
        {
            Dictionary<string, string> rusScripts = new Dictionary<string, string>
            {
                { "generateModelParams.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/generateModelParams.py" },
                { "text_expansion.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/text_expansion.py" }
            };

            foreach (var item in rusScripts)
            {
                if (!DownloadFile(item.Key, item.Value, path)) return false;
            }

            return true;
        }

        private bool DownloadEngScripts(string path)
        {
            Dictionary<string, string> engScripts = new Dictionary<string, string>();
            engScripts.Add("main.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/main.py");
            foreach (var item in engScripts)
            {
                if (!DownloadFile(item.Key, item.Value, path)) return false;
            }

            return true;
        }

        private bool DownloadGpt2(string path)
        {
            Dictionary<string, string> filesGpt2 = new Dictionary<string, string>
            {
                {
                    "__init__.py.txt",
                    "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/__init__.py.txt"
                },
                {
                    "config.n_ctx",
                    "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/config.n_ctx"
                },
                { "config.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/config.py" },
                { "encoder.json", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/encoder.json" },
                { "encoder.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/encoder.py" },
                { "model.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/model.py" },
                { "sample.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/sample.py" },
                { "utils.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/utils.py" },
                { "vocab.bpe", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/vocab.bpe" }
            };

            foreach (var item in filesGpt2)
            {
                if (!DownloadFile(item.Key, item.Value, path)) return false;
            }

            return true;
        }

        private bool DownloadFile(string fileName, string url, string savePath)
        {
            try
            {
                if (File.Exists(savePath + fileName)) return true;

                _webClient.DownloadFile(url, savePath + fileName);
                Logger.SaveLog($"Файл {fileName.Substring(0, 3)}...{fileName.Substring(fileName.Length - 1)} скачан успешно.", LogType.Info);
            }
            catch (Exception ex)
            {
                Logger.SaveLog($"Ошибка при скачке файла  {fileName.Substring(0, 3)}...{fileName.Substring(fileName.Length - 1)} - {ex.Message}", LogType.Error);
                return false;
            }

            return true;
        }
    }
}
