using System;
using log4net;
using NUnit.Core;

namespace NUnitAllureAdapter
{
    public class EventListenerAdapter : EventListener
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (EventListenerAdapter));

        public virtual void RunStarted(string name, int testCount)
        {
        }

        public virtual void RunFinished(TestResult result)
        {
        }

        public virtual void RunFinished(Exception exception)
        {
        }

        public virtual void TestStarted(TestName testName)
        {
        }

        public virtual void TestFinished(TestResult result)
        {
        }

        public virtual void SuiteStarted(TestName testName)
        {
        }

        public virtual void SuiteFinished(TestResult result)
        {
        }

        public virtual void UnhandledException(Exception exception)
        {
            Logger.Error(String.Format("UnhandledException"), exception);
        }

        public virtual void TestOutput(TestOutput testOutput)
        {
        }
    }
}