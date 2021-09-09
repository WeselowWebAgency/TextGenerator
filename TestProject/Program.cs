using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextGenerator;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = File.ReadAllText(@"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\TestProject\1.txt");
            PythonNetWorker worker2 = new PythonNetWorker(@"C:\Python37\", "python37.dll");
            string rez = worker2.GenerateEngText(text, @"C:\Users\Admin\Desktop\нейросетка\TextGenerator2\Gpt test\тест 2\");
            
            //worker2.Test2();

        }
    }
}
