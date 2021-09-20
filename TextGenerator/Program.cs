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
        private IZennoPosterProjectModel _project;
        private string Text { get; set; }


        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            _project = project;
            Logger.Project = project;
            int executionResult = 0;
            try
            {

                //получаем текст для работы и помещаем его в Text
                if (!GetTask(project.Variables["file_input"].Value)) return 1;

                //проверяем язык
                string language = project.Variables["language"].Value.ToLower();
                if (language != "rus" && language != "eng") return 1;

                //проверяем путь к питону
                string pythonPath = ValidatePythonPath(project.Variables["PythonPath"].Value)
                    ? project.Variables["PythonPath"].Value
                    : "";

                if (string.IsNullOrEmpty(pythonPath)) return 1;

                PythonNetWorker pythonNet = new PythonNetWorker(pythonPath, "python37.dll");
                Downloader worker = new Downloader(pythonPath);

                //скачиваем зависимости
                switch (_project.Variables["param_depends"].Value)
                {
                    case "startup":
                        if (!worker.CreateDirectories() || !worker.SaveScripts()
                            || !worker.DownloadPackages() || !worker.DownloadModels()) return 1;
                        _project.Variables["param_depends"].Value = "no_check";
                        break;
                    case "run":
                        if (!worker.CreateDirectories() || !worker.SaveScripts()
                            || !worker.DownloadPackages() || !worker.DownloadModels()) return 1;
                        break;
                    case "no_check":
                        break;
                    default:
                        break;
                }


                var par = SetParams(project);

                string rez = "";
                switch (language)
                {
                    case "rus":
                        rez = pythonNet.GenerateRusText(Text, par);
                        break;

                    case "eng":
                        rez = pythonNet.GenerateEngText(Text, par);
                        break;

                    default:
                        break;
                }
                executionResult = SaveResult(rez) /* & SaveToVariable("textResult", rez) */
                    ? 0     //все удачно
                    : 1;
            }
            catch (Exception e)
            {
                Logger.SaveLog(e.ToString(), LogType.Error);
                executionResult = 1;
                if (_project.Variables.Keys.Contains("errorText"))
                {
                    _project.Variables["errorText"].Value = e.Message;
                }
                else
                {
                    Logger.SaveLog("В проекте не создана переменная errorText, в которую можно записать текст ошибки", LogType.Error);
                }
                
            }

            return executionResult;
        }

        private bool ValidatePythonPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return false;

                path = Path.Combine(path, "python.exe");
                if (!File.Exists(path))
                {
                    Logger.SaveLog($"Не найден установленный {path}", LogType.Error);
                    return false;
                }
                else
                {
                    Logger.SaveLog($"Python найден - {path}", LogType.Info);
                }
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Не найден установленный {path} - {e.Message}", LogType.Error);
                return false;
            }
            return true;
        }
        private bool GetTask(string pathFile)
        {
            if (string.IsNullOrEmpty(pathFile))
            {
                Logger.SaveLog("Переменная pathFile пуста.", LogType.Error);
                return false;
            }

            if (!File.Exists(pathFile))
            {
                Logger.SaveLog($"Файл {pathFile} не существует.", LogType.Error);
                return false;
            }

            try
            {
                Text = File.ReadAllText(pathFile);
                if (Text.Length > 4)
                {
                    int len = Text.Length > 10 ? 10 : Text.Length;
                    Logger.SaveLog($"Задание из файла {pathFile} прочитано успешно - {Text.Substring(0, len)} ...",
                            LogType.Warning);
                }
                else
                {
                    Logger.SaveLog($"Задание из файла {pathFile} прочитано успешно, но оно короче 5 символов - {Text.Substring(0, 5)} ...", LogType.Error);
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Не смогли прочитать задание из файла {pathFile} - {e.Message} ...", LogType.Error);
                return false;
            }

            return true;
        }
        private bool SaveResult(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string filepath = _project.Variables["file_output"].Value;

            if (string.IsNullOrEmpty(filepath) || !filepath.Contains(":"))
            {
                //пусть пустой, используем по-умолчанию
                try
                {
                    string dir = Path.Combine(_project.Directory, "output");
                    filepath = Path.Combine(_project.Directory, "output", Path.GetFileName(_project.Variables["file_input"].Value));
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    Logger.SaveLog("Успешно создана директория output в папке проекта.", LogType.Info);
                }
                catch (Exception e)
                {
                    Logger.SaveLog($"Не смогли создать директорию output в папке проекта - {e.Message}.", LogType.Error);
                    return false;
                }
            }
            else
            {
                //путь не пустой, проверяем директорию на существование.
                try
                {
                    string dir = Path.GetDirectoryName(filepath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    Logger.SaveLog($"Успешно создана директория {dir}.", LogType.Info);
                }
                catch (Exception e)
                {
                    Logger.SaveLog($"Не смогли создать директорию для выходного файла - {e.Message}.", LogType.Error);
                    return false;
                }
            }

            //пробуем сохранить
            try
            {
                File.WriteAllText(filepath, text);
                Logger.SaveLog($"Текст успешно сохранен в файл {filepath}.", LogType.Warning);
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Ошибка при сохранении текста в файл {filepath} - {e.Message}.", LogType.Warning);
                return false;
            }

            return true;
        }
        private bool SaveToVariable(string variableName = "textResult", string text = "")
        {
            //проверяем существование переменной, если нет то создаем новую
            try
            {
                _project.Variables[variableName].Value = text;
                /*
                if (_project.Variables.Keys.Contains(variableName))
                {
                    _project.Variables[variableName].Value = text;
                }
                else
                {
                    object obj = _project.Variables;
                    obj.GetType().GetMethod("QuickCreateVariable").Invoke(obj, new Object[] { variableName });
                    _project.Variables[variableName].Value = text;
                    Logger.SaveLog($"Сохранили результат в переменную {variableName}", LogType.Warning);
                }*/
            }
            catch (Exception e)
            {
                Logger.SaveLog($"Не смогли создать переменную {variableName} для сохранения текста - {e.Message}", LogType.Error);
                return false;
            }

            return true;
        }
        private TextParams SetParams(IZennoPosterProjectModel project)
        {
            TextParams par = new TextParams();

            par.K = ConvertToInt(project.Variables["param_k"].Value, par.K);
            par.P = ConvertToDouble(project.Variables["param_p"].Value, par.P);

            par.Length = ConvertToInt(project.Variables["param_length"].Value, par.Length) > 512
                ? 512
                : ConvertToInt(project.Variables["param_length"].Value, par.Length);

            par.NumReturnSequences = ConvertToInt(project.Variables["param_NumReturnSequences"].Value, par.NumReturnSequences);
            par.Temperature = ConvertToDouble(project.Variables["param_temperature"].Value, par.Temperature);
            par.RepetitionPenalty = ConvertToDouble(project.Variables["param_RepetitionPenalty"].Value, par.RepetitionPenalty);

            if (project.Variables["param_ep"].Value.ToLower().Contains("true"))
            {
                par.Paraphrase = true;
                par.Expand = true;
            }
            else
            {
                par.Paraphrase = ConvertToBool(project.Variables["param_paraphrase"].Value, par.Paraphrase);
                par.Expand = ConvertToBool(project.Variables["param_expand"].Value, par.Expand);
            }

            return par;
        }
        private int ConvertToInt(string value, int defaultValue)
        {
            int rez = 0;
            try
            {
                rez = !string.IsNullOrEmpty(value)
                    ? Convert.ToInt32(value.Trim())
                    : defaultValue;
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Ошибка при конвертации {value} в число - {ex.Message}. Будет использовано значение по умолчанию {defaultValue}");
                rez = defaultValue;
            }

            return rez;
        }
        private Double ConvertToDouble(string value, double defaultValue)
        {
            double rez = 0;
            if (value.Contains(".")) value = value.Replace(".", ",");
            try
            {
                rez = !string.IsNullOrEmpty(value)
                    ? Convert.ToDouble(value.Trim())
                    : defaultValue;
            }

            catch (Exception ex)
            {
                _project.SendErrorToLog($"Ошибка при конвертации {value} в дробное число - {ex.Message}. Будет использовано значение по умолчанию {defaultValue}");
                rez = defaultValue;
            }

            return rez;
        }
        private bool ConvertToBool(string value, bool defaultValue)
        {
            bool rez;
            try
            {
                rez = !string.IsNullOrEmpty(value)
                    ? Convert.ToBoolean(value.Trim())
                    : defaultValue;
            }
            catch (Exception ex)
            {
                _project.SendErrorToLog($"Ошибка при конвертации {value} в bool - {ex.Message}. Будет использовано значение по умолчанию {defaultValue}");
                rez = defaultValue;
            }

            return rez;
        }
    }
}