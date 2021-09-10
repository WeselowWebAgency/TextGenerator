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


namespace TextGenerator
{
    public class Worker
    {

        private WebClient _webClient;
        private string path;
        //private IZennoPosterProjectModel  _project;
        private string _pythonPath;
        public Worker(string pythonPath)
        {
            _webClient = new WebClient();
            path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
            _pythonPath = pythonPath;
        }

        public Worker(IZennoPosterProjectModel project, string pythonPath)
        {
            _project = project;
        }

        private string StartProcess(ProcessStartInfo processInfo, string text)
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



        public void DownloadPackages()
        {

            //if ( SaveLog != null)  SaveLog("проверка обновления pip");
            string WorkingDirectory = $"{_pythonPath}\\Scripts";
            string argument = "install --upgrade pip command";
            string rezultProcess = StartProccess(argument, WorkingDirectory);
            //if ( SaveLog != null)  SaveLog($"проверка обновления закончена. Резуьтат {rezultProcess}");


            var packages = File.ReadAllLines(path + @"TextGenerator\requirements.txt");

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
            if (!File.Exists(path + @"TextGenerator\Assets\En\gpt2-pytorch_model.bin"))
            {
                //if ( SaveLog != null)  SaveLog("скачка моделей начата");

                try
                {
                    _webClient.DownloadFile("https://s3.amazonaws.com/models.huggingface.co/bert/gpt2-pytorch_model.bin", path + @"TextGenerator\Assets\En\gpt2-pytorch_model.bin");
                    SaveLog("скачка моделей закончена");
                }

                catch (Exception ex)
                {
                    SaveLog($" Cкачка моделей закончена с ошибкой {ex.Message}".ToUpper());
                }

            }

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
            return StartProcess(start, arg);
        }

        public void SaveLog(string text)
        {
            (_project != null) _project.SendInfoToLog(text);




        }

        public void SaveScripts()
        {
            
            string pathScripts = path  + "TextGenerator\\Assets\\";
            if (!Directory.Exists(pathScripts)) Directory.CreateDirectory(pathScripts);


            string pathEngScripts = pathScripts + "En\\";
            if (!Directory.Exists(pathEngScripts)) Directory.CreateDirectory(pathEngScripts);

            string GPT2path = pathEngScripts + "GPT2\\";
            if (!Directory.Exists(GPT2path)) Directory.CreateDirectory(GPT2path);

            string pathRuScripts = pathScripts + "Ru\\";
            if (!Directory.Exists(pathRuScripts)) Directory.CreateDirectory(pathRuScripts);



            DownloadRusScript(pathRuScripts);
            DownloadEngScripts(pathEngScripts);
            DownloadGPT2(GPT2path);

            //requirements


            DownloadScript("requirements.txt", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/requirements.txt", path + "TextGenerator\\");



        }

        private void DownloadRusScript(string path)
        {
            Dictionary<string, string> rusScripts = new Dictionary<string, string>();
            rusScripts.Add("generateModelParams.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/generateModelParams.py");
            rusScripts.Add("text_expansion.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/Ru/text_expansion.py");
            foreach (var item in rusScripts) DownloadScript(item.Key, item.Value, path);
        }

        private void DownloadEngScripts(string path)
        {
            Dictionary<string, string> engScripts = new Dictionary<string, string>();
            engScripts.Add("encoder.json", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/encoder.json");
            engScripts.Add("main.py", "https://raw.githubusercontent.com/WeselowWebAgency/TextGenerator/fork2/TextGenerator/Assets/En/main.py");

            foreach (var item in engScripts) DownloadScript(item.Key, item.Value, path);
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

            foreach (var item in filesGPT2) DownloadScript(item.Key, item.Value, path);
        }

        public void DownloadScript(string fileName, string url, string savePath)
        {
            if (!File.Exists(savePath + fileName))
            {
                string downloadText = _webClient.DownloadString(url);
                File.WriteAllText(savePath + fileName, downloadText);

            }
        }
    }
}
