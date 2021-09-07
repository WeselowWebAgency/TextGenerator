using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextGenerator
{
    class Worker
    {
        

        public string GenerateEngText(string text)
        {
            string scriptFileName = "main.py";

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Python37/python.exe";
            start.Arguments = string.Format("{0} {1}", scriptFileName, "--text " + '"' + text + '"');
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WorkingDirectory = @"C:\Users\Admin\Desktop\нейросетка\Gpt test\тест 2";
            return StartProcess(start);
        }

        public string GenerateRusText(string text)
        {
            string scriptFileName = "text_expansion.py";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:/Python37/python.exe";
            start.Arguments = string.Format("{0} {1}", scriptFileName, "--text " + '"' + text + '"');
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.WorkingDirectory = @"C:\Users\Admin\Desktop\нейросетка\Rugpt";
            return StartProcess(start);

        }

        private string StartProcess(ProcessStartInfo processInfo) {
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

    }
}
