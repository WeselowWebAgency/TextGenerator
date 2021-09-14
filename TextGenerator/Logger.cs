using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            return true;
        }
    }
}
