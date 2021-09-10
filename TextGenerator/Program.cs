using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using ZennoLab.CommandCenter;
using ZennoLab.Emulation;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;

namespace TextGenerator
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode
    {
        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            int executionResult = 0;

            string PathFile = project.Variables["pathFile"].Value;
            string text = File.ReadAllText(PathFile);
            
            string language = project.Variables["language"].Value;
            string pathFileRezult = project.Variables["pathFileRezult"].Value;

            
            string rugptpPath = project.Variables["RugptPath"].Value;
            string generateEngScriptPath = project.Variables["EngScriptPath"].Value;

            string rez = "";

            string pythonPath = project.Variables["PythonPath"].Value;
            PythonNetWorker PythonNet = new PythonNetWorker(pythonPath, "python37.dll");

            

            Worker worker = new Worker(project, pythonPath);
            worker.SaveScripts();
            worker.DownloadPackages();
            worker.DownloadModels();





            switch (language) {
                case "rus":
                    rez = PythonNet.GenerateRusText(text);
                    File.WriteAllText(pathFileRezult, rez);
                    break;
                
                case "eng":
                    rez = PythonNet.GenerateEngText(text);
                    File.WriteAllText(pathFileRezult, rez);
                    break;
                default:

                    break;
            }


            return executionResult;
        }
    }
}