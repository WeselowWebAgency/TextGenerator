using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TextGenerator.Models;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace TextGenerator
{
    public static class Logger
    {
        public static IZennoPosterProjectModel Project { get; set; }

        public static bool SaveLog(string text = "No-log-text-submitted", LogType logType = LogType.Error)
        {
            if (Project != null)
            {
                switch (logType)
                {
                    case LogType.Error:
                        Project.SendErrorToLog(text);
                        break;

                    case LogType.Info:
                        Project.SendInfoToLog(text);
                        break;

                    case LogType.Warning:
                        Project.SendWarningToLog(text);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
                }

            }

            else Console.WriteLine(text);

            SaveToFile($"Log Date:{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n" +
                       $"Log Type: {logType.ToString()}\n\n" +
                       $"{text}\n" +
                       $"\n=============================================\n\n");

            return true;
        }

        private static bool SaveToFile(string text)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.AppendAllText(Path.Combine(Project.Directory, "_logs.txt"), text);
                    Thread.Sleep(1000);
                    break;
                }
                catch (Exception e)
                {
                    Project.SendErrorToLog($"Не смогли сохранить лог-запись в файл _logs.txt, попытка {i}/3.", true);
                    Thread.Sleep(1000);
                }
            }
            return true;
        }
    }
}
