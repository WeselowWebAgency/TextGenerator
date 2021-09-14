using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextGenerator;
using TextGenerator.Models;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Generate();
        }

        public static void Download() {
            Downloader worker = new Downloader(@"C:\Python37");

            string path = @"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\TextGenerator\";
            //worker.DownloadPackages(path);
            if (worker.CreateDirectories()) {
                
                worker.SaveScripts();
                worker.DownloadPackages();
                worker.DownloadModels();
            }

            
            
        }

        public static void Generate() {
            TextParams par = new TextParams();

            string engText = File.ReadAllText(@"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\1.txt");
            string rusText = File.ReadAllText(@"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\2.txt");
            PythonNetWorker worker2 = new PythonNetWorker(@"C:\Python37\", "python37.dll");

            
            string rez = worker2.GenerateRusText(rusText, par);

        }
    }
}
