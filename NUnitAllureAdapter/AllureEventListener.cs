using System;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using AllureCSharpCommons;
using AllureCSharpCommons.Events;
using AllureCSharpCommons.Utils;
using log4net;
using NUnit.Core;
using NUnit.Framework;

namespace NUnitAllureAdapter
{
    public class AllureEventListener : EventListener
    {
        private readonly Allure _lifecycle = Allure.Lifecycle;

        private static readonly ILog logger = LogManager.GetLogger(typeof(AllureEventListener));
        
        private static readonly OrderedDictionary SuiteStorage =
            new OrderedDictionary();

        private StringBuilder _stdOut = new StringBuilder();
        private StringBuilder _trace = new StringBuilder();
        private StringBuilder _log = new StringBuilder();
        private StringBuilder _stdErr = new StringBuilder();
        
        static AllureEventListener()
        {
            try
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

                AllureConfig.ResultsPath = XDocument.Load(path + "/config.xml").Descendants().First().Value
                    + Path.DirectorySeparatorChar;
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Exception in initialization"), e);
            }
        }

        public void RunStarted(string name, int testCount)
        {
            try
            {
                if (Directory.Exists(AllureConfig.ResultsPath))
                {
                    Directory.Delete(AllureConfig.ResultsPath, true);
                }
                Directory.CreateDirectory(AllureConfig.ResultsPath);
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Exception in RunStarted {0}", name), e);
            }
        }

        public void RunFinished(TestResult result)
        {
        }

        public void RunFinished(Exception exception)
        {
        }

        public void TestStarted(TestName testName)
        {
            try
            {
                var assembly = testName.FullName.Split('.')[0];
                var clazz = testName.FullName.Split('.')[testName.FullName.Split('.').Count() - 2];

                var evt = new TestCaseStartedEvent((string)SuiteStorage[SuiteStorage.Count - 1], testName.FullName);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains(assembly)))
                {
                    foreach (var type in asm.GetTypes().Where(x => x.Name.Contains(clazz)))
                    {
                        var name = !testName.Name.Contains("(")
                            ? testName.Name
                            : testName.Name.Substring(0, testName.Name.IndexOf('('));

                        var methodInfo = type.GetMethod(name);
                        if (methodInfo == null) continue;
                        var manager =
                            new AttributeManager(methodInfo.GetCustomAttributes(false).OfType<Attribute>().ToList());
                        manager.Update(evt);
                    }
                }

                _lifecycle.Fire(evt);
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Exception in TestStarted {0}", testName), e);
            }
        }

        public void TestFinished(TestResult result)
        {
            try
            {
                if (result.IsError)
                {
                    TakeScreenshot();
                    _lifecycle.Fire(new TestCaseFailureEvent
                    {
                        Throwable = new Exception(result.Message),
                        StackTrace = result.StackTrace
                    });
                }
                else if (result.IsFailure)
                {
                    TakeScreenshot();
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
            catch (Exception e)
            {
                logger.Error(String.Format("Exception in TestFinished \"{0}\"", result), e);
            }
        }

        public void SuiteStarted(TestName testName)
        {
            try
            {
                var assembly = testName.FullName.Split('.')[0];
                var clazz = testName.FullName.Split('.')[testName.FullName.Split('.').Count() - 1];

                var suiteUid = Guid.NewGuid().ToString();
                var evt = new TestSuiteStartedEvent(suiteUid, testName.FullName);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains(assembly)))
                {
                    foreach (var type in asm.GetTypes().Where(x => x.Name.Contains(clazz)))
                    {
                        var manager = new AttributeManager(type.GetCustomAttributes(false).OfType<Attribute>().ToList());
                        manager.Update(evt);
                    }
                }

                SuiteStorage.Add(testName.FullName, suiteUid);

                _lifecycle.Fire(evt);
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Exception in SuiteStarted \"{0}\"", testName), e);
            }
        }

        public void SuiteFinished(TestResult result)
        {
            try
            {
                _lifecycle.Fire(new TestSuiteFinishedEvent((string)SuiteStorage[result.FullName]));
                SuiteStorage.Remove(result.FullName);
            }
            catch (Exception e)
            {
                logger.Error(String.Format("Exception in SuiteFinished \"{0}\"", result), e);
            }
        }

        public void UnhandledException(Exception exception)
        {
            logger.Error(String.Format("UnhandledException"), exception);
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

        private void TakeScreenshot()
        {
            _lifecycle.Fire(new MakeAttachmentEvent(AllureResultsUtils.TakeScreenShot(), "Screenshot", "image/png"));
        }
    }
}