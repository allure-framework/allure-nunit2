using System;
using System.Collections.Generic;
using System.IO;
using AllureCSharpCommons;
using AllureCSharpCommons.Events;
using AllureCSharpCommons.Exceptions;
using NUnit.Core;

namespace NUnitAllureAdapter
{
    public class AllureEventListener : EventListener
    {
        private readonly Allure _lifecycle = Allure.Lifecycle;
        
        private readonly Dictionary<string, string> _suiteStorage = 
            new Dictionary<string, string>();

        private readonly FileInfo _file = new FileInfo("out.txt");

        private void Write(string text)
        {
            StreamWriter sw = _file.AppendText();
            sw.WriteLine(text);
            sw.Close();
        }

        public void RunStarted(string name, int testCount)
        {
            Write("Run Started");
        }

        public void RunFinished(TestResult result)
        {
            Write("Run Finished result");
        }

        public void RunFinished(Exception exception)
        {
            Write("Run Finished exception");
        }

        public void TestStarted(TestName testName)
        {
            Write("Test Started");
        }

        public void TestFinished(TestResult result)
        {
            Write("Test Finished");
        }

        public void SuiteStarted(TestName testName)
        {
            Write("Suite Started");
            var suiteUid = Guid.NewGuid().ToString();
            _suiteStorage.Add(testName.FullName, suiteUid);

            TestSuiteStartedEvent evt =
                new TestSuiteStartedEvent(suiteUid, testName.FullName);

            _lifecycle.Fire(evt);
        }

        public void SuiteFinished(TestResult result)
        {
            Write("Suite Finished");
            if (!_suiteStorage.ContainsKey(result.FullName))
            {
                throw new AllureException("");
            }

            TestSuiteFinishedEvent evt =
                new TestSuiteFinishedEvent(_suiteStorage[result.FullName]);

            _lifecycle.Fire(evt);
        }

        public void UnhandledException(Exception exception)
        {
        }

        public void TestOutput(TestOutput testOutput)
        {
        }
    }
}