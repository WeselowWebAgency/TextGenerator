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
        /// 

        IZennoPosterProjectModel _project;
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            int executionResult = 0;
            _project = project;
            
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

            par.K = ConvertToInt(project.Variables["K"].Value, par.K);
            par.P = ConvertToDouble(project.Variables["P"].Value, par.P);
            par.Length = ConvertToInt(project.Variables["Length"].Value, par.Length);
            par.NumReturnSequences = ConvertToInt(project.Variables["NumReturnSequences"].Value, par.NumReturnSequences);
            par.Temperature = ConvertToDouble(project.Variables["Temperature"].Value, par.Temperature);
            par.RepetitionPenalty = ConvertToDouble(project.Variables["RepetitionPenalty"].Value, par.RepetitionPenalty);
            par.paraphrase = ConvertToBool(project.Variables["paraphrase"].Value, par.paraphrase);
            par.expand = ConvertToBool(project.Variables["expand"].Value, par.expand);
            return par;

        }

        private int ConvertToInt(string value,int defaultValue) {
            int rez = 0;
            try
            {
                rez = string.IsNullOrEmpty(value) ? Convert.ToInt32(_project.Variables["k"].Value.Trim()) : defaultValue;
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Ошибка при конвертации {value} в число { ex.Message} ,будет использовано значение по умолчанию {defaultValue}");
            }
            return rez;
        }

        private Double ConvertToDouble(string value, double defaultValue) {
            double rez = 0;
            try
            {
                rez = string.IsNullOrEmpty(value) ? Convert.ToDouble(_project.Variables["k"].Value.Trim()) : defaultValue;
            }
            
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Ошибка при конвертации {value} в дробное число { ex.Message} ,будет использовано значение по умолчанию {defaultValue}");
            }
            return rez;

        }

        public bool ConvertToBool(string value, bool defaultValue) {
            bool rez = true; ;
            try
            {
                rez = string.IsNullOrEmpty(value) ? Convert.ToBoolean(_project.Variables["k"].Value.Trim()) : defaultValue;
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Ошибка при конвертации {value} в булевое значение { ex.Message} ,будет использовано значение по умолчанию {defaultValue}");
                rez = !rez;
            }
            return rez;
        }
    }


}