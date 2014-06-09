using System;
using System.IO;
using NUnit.Core;

namespace NUnitAllureAdapter
{
    public class AllureEventListener : EventListener
    {
        private void Write()
        {
            StreamWriter streamWriter = null;
            try
            {
                string xmlString = "42\n";
                var xmlFile = new FileInfo("out.txt");
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        public void RunStarted(string name, int testCount)
        {
            Write();
        }

        public void RunFinished(TestResult result)
        {
            Write();
        }

        public void RunFinished(Exception exception)
        {
            Write();
        }

        public void TestStarted(TestName testName)
        {
            Write();
        }

        public void TestFinished(TestResult result)
        {
            Write();
        }

        public void SuiteStarted(TestName testName)
        {
            Write();
        }

        public void SuiteFinished(TestResult result)
        {
            Write();
        }

        public void UnhandledException(Exception exception)
        {
            Write();
        }

        public void TestOutput(TestOutput testOutput)
        {
            Write();
        }
    }
}
