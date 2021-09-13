using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace TextGenerator
{
    public class Downloader
    {

        private WebClient _webClient;
        private string _path;
        private IZennoPosterProjectModel _project;
        private string _pythonPath;
        public Downloader(string pythonPath)
        {
            _webClient = new WebClient();
            //_path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
            _path = Path.GetTempPath();
            
            _pythonPath = pythonPath;
            _project = null;
        }

        public Downloader(IZennoPosterProjectModel project, string pythonPath)
        {
            _pythonPath = pythonPath;
            _project = project;
        }

        private string StartProcess(ProcessStartInfo processInfo)
        {
            string result;
            using (Process process = Process.Start(processInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd();
                    Console.Write(result);

                }
            }
            return result;
        }

        public void CreateDirectories()
        {
            string baseFolder = _path + "TextGenerator\\";
            if (Directory.Exists(baseFolder)) Directory.CreateDirectory(baseFolder);

            string assetsPath = baseFolder + "Assets\\";
            if (Directory.Exists(assetsPath)) Directory.CreateDirectory(assetsPath);

            string enScripts = assetsPath + "En\\";
            if (Directory.Exists(enScripts)) Directory.CreateDirectory(enScripts);

            string ruScripts = assetsPath + "Ru\\";
            if (Directory.Exists(ruScripts)) Directory.CreateDirectory(ruScripts);

            string GPT2path = enScripts + "GPT2\\";
            if (!Directory.Exists(GPT2path)) Directory.CreateDirectory(GPT2path);
        }

        public void DownloadPackages()
        {
            CreateDirectories();

            SaveLog("скачка зависимостей начата");

            SaveLog("проверка обновления pip");
            string WorkingDirectory = $"{_pythonPath}\\Scripts";
            string argument = "install --upgrade pip command";
            string rezultProcess = StartProccess(argument, WorkingDirectory);
            SaveLog($"проверка обновления закончена. Резуьтат {rezultProcess}");


            var packages = File.ReadAllLines(_path + @"TextGenerator\requirements.txt");

            for (int i = 0; i < packages.Length; i++)
            {
                SaveLog($"Проверка зависимости  {packages[i]}");
                argument = $"install {packages[i]}";
                rezultProcess = StartProccess(argument, null);
                SaveLog($"Результат Проверки зависимости  {packages[i]}: {rezultProcess}");
            }

            SaveLog($"Проверка зависимости  xx_ent_wiki_sm ");
            argument = $"-m spacy download xx_ent_wiki_sm";
            rezultProcess = StartProccess(argument, null, @"\python.exe");
            SaveLog($"Результат Проверки зависимости  xx_ent_wiki_sm: {rezultProcess}");



            SaveLog($"Проверка зависимости  squad_bert ");
            argument = $"-m deeppavlov install squad_bert ";
            rezultProcess = StartProccess(argument, null, @"\python.exe");
            SaveLog($"Результат Проверки зависимости  squad_bert: {rezultProcess}");



            SaveLog("скачка зависимостей закончена");



        }

        public void DownloadModels()
        {
            string path = _path + @"TextGenerator\Assets\En\";
           
            SaveLog("скачка моделей начата");
            DownloadFile("gpt2-pytorch_model.bin", "https://s3.amazonaws.com/models.huggingface.co/bert/gpt2-pytorch_model.bin", path);
            SaveLog("скачка моделей закончена");
        }

        public string StartProccess(string arg, string WorkingDirectory, string fileName = @"\Scripts\pip.exe")
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

        public void SaveLog(string text)
        {
            if (_project != null) _project.SendInfoToLog(text);
            Console.WriteLine(text);
        }

        public void SaveScripts()
        {

            string pathScripts = _path + "TextGenerator\\Assets\\";
            string pathEngScripts = pathScripts + "En\\";
            string GPT2path = pathEngScripts + "GPT2\\";
            string pathRuScripts = pathScripts + "Ru\\";

            CreateDirectories();

            SaveLog($" Cкачка скриптов начата");
            DownloadRusScripts(pathRuScripts);
            DownloadEngScripts(pathEngScripts);
            DownloadGPT2(GPT2path);
            DownloadFile("requirements.txt", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/requirements.txt", _path + "TextGenerator\\");
            SaveLog($" Cкачка скриптов закочена");
        }

        private void DownloadRusScripts(string path)
        {
            Dictionary<string, string> rusScripts = new Dictionary<string, string>();
            rusScripts.Add("generateModelParams.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/generateModelParams.py");
            rusScripts.Add("text_expansion.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/text_expansion.py");
            foreach (var item in rusScripts) DownloadFile(item.Key, item.Value, path);
        }

        private void DownloadEngScripts(string path)
        {
            Dictionary<string, string> engScripts = new Dictionary<string, string>();
            engScripts.Add("main.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/main.py");
            foreach (var item in engScripts) DownloadFile(item.Key, item.Value, path);
        }

        private void DownloadGPT2(string path)
        {
            Dictionary<string, string> filesGPT2 = new Dictionary<string, string>();
            filesGPT2.Add("__init__.py.txt", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/__init__.py.txt");
            filesGPT2.Add("config.n_ctx", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/config.n_ctx");
            filesGPT2.Add("config.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/config.py");
            filesGPT2.Add("encoder.json", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/encoder.json");
            filesGPT2.Add("encoder.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/encoder.py");
            filesGPT2.Add("model.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/model.py");
            filesGPT2.Add("sample.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/sample.py");
            filesGPT2.Add("utils.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/utils.py");
            filesGPT2.Add("vocab.bpe", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/GPT2/vocab.bpe");

            foreach (var item in filesGPT2) DownloadFile(item.Key, item.Value, path);
        }

        private void DownloadFile(string fileName, string url, string savePath)
        {
            try
            {
                if (!File.Exists(savePath + fileName))
                {
                    _webClient.DownloadFile(url, savePath + fileName);

                    SaveLog($"файл {fileName} скачан");
                }
            }
            catch (Exception ex)
            {
                SaveLog($"ошибка при скачке файла {fileName}: {ex.Message}");
            }
        }
    }
}
