using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextGenerator.Models;

namespace TextGenerator.Tests
{
    [TestClass()]
    public class PythonNetWorkerTests
    {
        [TestMethod()]
        public void GenerateEngTextTest()
        {
            try
            {
                var worker = new PythonNetWorker(@"c:\Python37", "python37.dll");
                string text = "It was a bright cold day in April.";
                string result = worker.GenerateEngText(text, new TextParams());
                Debug.Write(result);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                Assert.Fail();
            }

        }

        [TestMethod()]
        public void GenerateRusTextTest()
        {
            try
            {
                var worker = new PythonNetWorker(@"c:\Python37", "python37.dll");
                string text = "Лето – это время каникул и отпусков.";
                string result = worker.GenerateEngText(text, new TextParams());
                Debug.Write(result);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                Assert.Fail();
            }
        }
    }
}