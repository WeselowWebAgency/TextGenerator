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
    public class Worker
    {

        private WebClient _webClient;
        private string path;
        private IZennoPosterProjectModel _project;
        private string _pythonPath;
        public Worker(string pythonPath) {
            _webClient = new WebClient();
            path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
            _pythonPath = pythonPath;
        }

        public Worker(IZennoPosterProjectModel project, string pythonPath) {
            _project = project;
        }
        
        private string StartProcess(ProcessStartInfo processInfo,string text) {
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

        

        public void DownloadPackages() {

            if (_project != null) _project.SendInfoToLog("проверка обновления pip");
            string WorkingDirectory = @"C:\Python37\Scripts";
            string argument = "install --upgrade pip command";
            string rezultProcess = StartProccess(argument, WorkingDirectory);
            if (_project != null) _project.SendInfoToLog($"проверка обновления закончена. Резуьтат {rezultProcess}");


            var packages = File.ReadAllLines(path + @"TextGenerator\requirements.txt");
            
            for (int i = 0; i < packages.Length; i++) {
                if (_project != null) _project.SendInfoToLog($"Проверка зависимости  {packages[i]}") ;
                argument = $"install {packages[i]}";
                rezultProcess = StartProccess(argument, null);
                if (_project != null) _project.SendInfoToLog($"Результат Проверки зависимости  {packages[i]}: {rezultProcess}");
            }

            if (_project != null) _project.SendInfoToLog($"Проверка зависимости  xx_ent_wiki_sm ");
            argument = $"-m spacy download xx_ent_wiki_sm";
            rezultProcess = StartProccess(argument, null, @"\python.exe");
            if (_project != null) _project.SendInfoToLog($"Результат Проверки зависимости  xx_ent_wiki_sm: {rezultProcess}");



            if (_project != null) _project.SendInfoToLog($"Проверка зависимости  squad_bert ");
            argument = $"-m deeppavlov install squad_bert ";
            rezultProcess = StartProccess(argument, null, @"\python.exe");
            if (_project != null) _project.SendInfoToLog($"Результат Проверки зависимости  squad_bert: {rezultProcess}");



            if (_project != null) _project.SendInfoToLog("скачка зависимостей закончена");



        }

        public void DownloadModels() {
            if (!File.Exists(path + @"TextGenerator\Assets\En\gpt2-pytorch_model.bin")) {
                if (_project != null) _project.SendInfoToLog("скачка моделей начата");

                try
                {
                    _webClient.DownloadFile("https://s3.amazonaws.com/models.huggingface.co/bert/gpt2-pytorch_model.bin", path + @"TextGenerator\Assets\En\gpt2-pytorch_model.bin");
                    if (_project != null) _project.SendInfoToLog("скачка моделей закончена");
                }

                catch (Exception ex) {
                    if (_project != null) _project.SendInfoToLog($" Cкачка моделей закончена с ошибкой {ex.Message}".ToUpper());
                }
                
            }
           
        }

        public string StartProccess(string arg, string WorkingDirectory, string fileName = @"\Scripts\pip.exe") {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _pythonPath + fileName;
            start.Arguments = string.Format("{0}", arg);
            
            start.RedirectStandardInput = true;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WorkingDirectory = WorkingDirectory;
            return StartProcess(start, arg);
        }
    }
}
