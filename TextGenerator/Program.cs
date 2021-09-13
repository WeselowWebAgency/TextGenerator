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
using TextGenerator.Models;
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


            string pythonPath = project.Variables["PythonPath"].Value;
            PythonNetWorker PythonNet = new PythonNetWorker(pythonPath, "python37.dll");


            string rez = "";
            Downloader worker = new Downloader(project, pythonPath);
            worker.SaveScripts();
            worker.DownloadPackages();
            worker.DownloadModels();





            TextParams par = new TextParams()
            {
                K = Convert.ToInt32(project.Variables["k"].Value),
                P = Convert.ToInt32(project.Variables["p"].Value),
                Length = Convert.ToInt32(project.Variables["Length"].Value),
                NumReturnSequences = Convert.ToInt32(project.Variables["NumReturnSequences"].Value),
                Temperature = Convert.ToInt32(project.Variables["Temperature"].Value),
                RepetitionPenalty = Convert.ToDouble(project.Variables["RepetitionPenalty"].Value)
            };




            switch (language)
            {
                case "rus":
                    rez = PythonNet.GenerateRusText(text, par);
                    File.WriteAllText(pathFileRezult, rez);
                    break;

                case "eng":
                    rez = PythonNet.GenerateEngText(text, par);
                    File.WriteAllText(pathFileRezult, rez);
                    break;
                default:

                    break;
            }


            return executionResult;
        }
    }
}