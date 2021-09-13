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


            Downloader worker = new Downloader(@"C:\Python37");

            string path = @"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\TextGenerator\";
            //worker.DownloadPackages(path);
            if (worker.CreateDirectories() == true)
            {

                worker.SaveScripts();
                worker.DownloadPackages();
                worker.DownloadModels();
            }










            switch (language)
            {
                case "rus":
                    rez = PythonNet.GenerateRusText(text, SetParams(project));
                    File.WriteAllText(pathFileRezult, rez);
                    break;

                case "eng":
                    rez = PythonNet.GenerateEngText(text, SetParams(project));
                    File.WriteAllText(pathFileRezult, rez);
                    break;
                default:

                    break;
            }


            return executionResult;
        }
        public TextParams SetParams(IZennoPosterProjectModel project)
        {
            TextParams par = new TextParams();
            try
            {
                par.K = string.IsNullOrEmpty(project.Variables["k"].Value) ? Convert.ToInt32(project.Variables["k"].Value.Trim()) : par.K;
            }
            catch (Exception ex )
            {
                project.SendErrorToLog("Ошибка при конвертации {ex}",ex.Message);
            }
            try{ 
                par.P =  string.IsNullOrEmpty(project.Variables["p"].Value) ? Convert.ToInt32(project.Variables["p"].Value.Trim()) : par.P; 
            }
            catch (Exception ex)
            {
                project.SendErrorToLog("Ошибка при конвертации {ex}", ex.Message);
            }
            try
            {
                par.Length = string.IsNullOrEmpty(project.Variables["Length"].Value) ? Convert.ToInt32(project.Variables["Length"].Value.Trim()) : par.Length;
            }
            catch (Exception ex)
            {
                project.SendErrorToLog("Ошибка при конвертации {ex}", ex.Message);
            }
            try
            {
                par.NumReturnSequences = string.IsNullOrEmpty(project.Variables["NumReturnSequences"].Value.Trim()) ? Convert.ToInt32(project.Variables["NumReturnSequences"].Value) : par.NumReturnSequences;
            }
            catch (Exception ex)
            {
                project.SendErrorToLog("Ошибка при конвертации {ex}", ex.Message);
            }
            try
            {
                par.Temperature = string.IsNullOrEmpty(project.Variables["Temperature"].Value.Trim()) ? Convert.ToInt32(project.Variables["Temperature"].Value) : par.Temperature;
            }
            catch (Exception ex) {
                project.SendErrorToLog("Ошибка при конвертации {ex}", ex.Message);
            }
            try {
                par.RepetitionPenalty = string.IsNullOrEmpty(project.Variables["RepetitionPenalty"].Value.Trim()) ? Convert.ToDouble(project.Variables["RepetitionPenalty"].Value) : par.RepetitionPenalty;
            }
            catch (Exception ex) {
                project.SendErrorToLog("Ошибка при конвертации {ex}", ex.Message);
            }
            return par;

        }


    }


}