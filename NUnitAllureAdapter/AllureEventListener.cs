using System;
using System.Collections.Specialized;
using System.Text;
using AllureCSharpCommons;
using AllureCSharpCommons.Events;
using log4net;
using NUnit.Core;
using NUnit.Framework;

namespace NUnitAllureAdapter
{
    public class AllureEventListener : EventListener
    {
        private readonly Allure _lifecycle = Allure.Lifecycle;
        
        private static readonly OrderedDictionary SuiteStorage =
            new OrderedDictionary();

        private StringBuilder _stdOut = new StringBuilder();
        private StringBuilder _trace = new StringBuilder();
        private StringBuilder _log = new StringBuilder();
        private StringBuilder _stdErr = new StringBuilder();

        private readonly static ILog Log = LogManager.GetLogger(typeof(Allure));
        
        public void RunStarted(string name, int testCount)
        {
        }

        public void RunFinished(TestResult result)
        {
        }

        public void RunFinished(Exception exception)
        {
        }

        public void TestStarted(TestName testName)
        {
            _lifecycle.Fire(new TestCaseStartedEvent((string) SuiteStorage[SuiteStorage.Count - 1], testName.FullName));
        }

        public void TestFinished(TestResult result)
        {
            if (result.IsError)
            {
                _lifecycle.Fire(new TestCaseFailureEvent
                {
                    Throwable = new Exception(result.Message),
                    StackTrace = result.StackTrace
                });
            }
            else if (result.IsFailure)
            {
                _lifecycle.Fire(new TestCaseFailureEvent
                {
                    Throwable = new AssertionException(result.Message),
                    StackTrace = result.StackTrace
                });
            }
            else if (!result.Executed)
            {
                if (result.ResultState == ResultState.Cancelled)
                {
                    _lifecycle.Fire(new TestCaseCanceledEvent
                    {
                        Throwable = new Exception(result.Message),
                        StackTrace = result.StackTrace
                    });
                }
                else
                {
                    _lifecycle.Fire(new TestCasePendingEvent
                    {
                        Throwable = new Exception(result.Message),
                        StackTrace = result.StackTrace
                    });
                }
            }
            WriteOutputToAttachment();
            _lifecycle.Fire(new TestCaseFinishedEvent());
        }

        public void SuiteStarted(TestName testName)
        {
            var suiteUid = Guid.NewGuid().ToString();
            SuiteStorage.Add(testName.FullName, suiteUid);

            _lifecycle.Fire(new TestSuiteStartedEvent(suiteUid, testName.FullName));
        }

        public void SuiteFinished(TestResult result)
        {
            _lifecycle.Fire(new TestSuiteFinishedEvent((string) SuiteStorage[result.FullName]));
            SuiteStorage.Remove(result.FullName);
        }

        public void UnhandledException(Exception exception)
        {
        }

        public void TestOutput(TestOutput testOutput)
        {
            switch (testOutput.Type)
            {
                case TestOutputType.Out:
                    _stdOut.Append(testOutput.Text);
                    break;
                case TestOutputType.Trace:
                    _trace.Append(testOutput.Text);
                    break;
                case TestOutputType.Log:
                    _log.Append(testOutput.Text);
                    break;
                case TestOutputType.Error:
                    _stdErr.Append(testOutput.Text);
                    break;
            }
        }

        private void WriteOutputToAttachment()
        {
            if (_stdOut.Length > 0)
            {
                _lifecycle.Fire(new MakeAttachmentEvent(Encoding.UTF8.GetBytes(_stdOut.ToString()), "StdOut", "text/plain"));
            }
            if (_trace.Length > 0)
            {
                _lifecycle.Fire(new MakeAttachmentEvent(Encoding.UTF8.GetBytes(_trace.ToString()), "Trace", "text/plain"));
            }
            if (_log.Length > 0)
            {
                _lifecycle.Fire(new MakeAttachmentEvent(Encoding.UTF8.GetBytes(_log.ToString()), "Log", "text/plain"));
            }
            if (_stdErr.Length > 0)
            {
                _lifecycle.Fire(new MakeAttachmentEvent(Encoding.UTF8.GetBytes(_stdErr.ToString()), "StdErr", "text/plain"));
            }
            _stdOut = new StringBuilder();
            _trace = new StringBuilder();
            _log = new StringBuilder();
            _stdErr = new StringBuilder();
        }
    }
}