using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
    public class AllureEventListener : EventListenerAdapter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (AllureEventListener));

        private static readonly OrderedDictionary SuiteStorage =
            new OrderedDictionary();

        private static readonly bool WriteOutputToAttachmentFlag;
        private static readonly bool TakeScreenShotOnFailedTestsFlag;

        private readonly Allure _lifecycle = Allure.Lifecycle;

        private StringBuilder _log = new StringBuilder();
        private StringBuilder _stdErr = new StringBuilder();
        private StringBuilder _stdOut = new StringBuilder();
        private StringBuilder _trace = new StringBuilder();

        static AllureEventListener()
        {
            try
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

                AllureConfig.ResultsPath =
                    XDocument.Load(path + "/config.xml")
                        .Descendants()
                        .First(x => x.Name.LocalName.Equals("results-path"))
                        .Value + "/";

                TakeScreenShotOnFailedTestsFlag =
                    Convert.ToBoolean(XDocument.Load(path + "/config.xml")
                        .Descendants()
                        .First(x => x.Name.LocalName.Equals("take-screenshots-after-failed-tests"))
                        .Value);

                WriteOutputToAttachmentFlag =
                    Convert.ToBoolean(XDocument.Load(path + "/config.xml")
                        .Descendants()
                        .First(x => x.Name.LocalName.Equals("write-output-to-attachment"))
                        .Value);

                Logger.Error("Initialization completed successfully.\n");
                Logger.Error(
                    String.Format(
                        "Results Path: {0};\n WriteOutputToAttachmentFlag: {1};\n TakeScreenShotOnFailedTestsFlag: {2}",
                        AllureConfig.ResultsPath, WriteOutputToAttachmentFlag, TakeScreenShotOnFailedTestsFlag));

                if (Directory.Exists(AllureConfig.ResultsPath))
                {
                    Directory.Delete(AllureConfig.ResultsPath, true);
                }
                Directory.CreateDirectory(AllureConfig.ResultsPath);
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Exception in initialization"), e);
            }
        }

        public override void TestStarted(TestName testName)
        {
            try
            {
                string assembly = testName.FullName.Split('.')[0];
                string clazz = testName.FullName.Split('(')[0].Split('.')[testName.FullName.Split('(')[0].Split('.').Count() - 2];

                var evt = new TestCaseStartedEvent((string) SuiteStorage[SuiteStorage.Count - 1], testName.FullName);

                foreach (
                    Assembly asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains(assembly)))
                {
                    foreach (Type type in asm.GetTypes().Where(x => x.Name.Contains(clazz)))
                    {
                        string name = !testName.Name.Contains("(")
                            ? testName.Name
                            : testName.Name.Substring(0, testName.Name.IndexOf('('));

                        MethodInfo methodInfo = type.GetMethod(name);
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
                Logger.Error(String.Format("Exception in TestStarted {0}", testName), e);
            }
        }

        public override void TestFinished(TestResult result)
        {
            try
            {
                if (result.IsError)
                {
                    if (TakeScreenShotOnFailedTestsFlag)
                    {
                        TakeScreenshot();
                    }
                    _lifecycle.Fire(new TestCaseFailureEvent
                    {
                        Throwable = new Exception(result.Message),
                        StackTrace = result.StackTrace
                    });
                }
                else if (result.IsFailure)
                {
                    if (TakeScreenShotOnFailedTestsFlag)
                    {
                        TakeScreenshot();
                    }
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
                Logger.Error(String.Format("Exception in TestFinished \"{0}\"", result), e);
            }
        }

        public override void SuiteStarted(TestName testName)
        {
            try
            {
                string assembly = testName.FullName.Split('.')[0];
                string clazz = testName.FullName.Split('.')[testName.FullName.Split('.').Count() - 1];

                string suiteUid = Guid.NewGuid().ToString();
                var evt = new TestSuiteStartedEvent(suiteUid, testName.FullName);

                foreach (
                    Assembly asm in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains(assembly)))
                {
                    foreach (Type type in asm.GetTypes().Where(x => x.Name.Equals(clazz)))
                    {
                        var manager = new AttributeManager(type.GetCustomAttributes(false).OfType<Attribute>().ToList());
                        manager.Update(evt);

                        SuiteStorage.Add(testName.FullName, suiteUid);

                        _lifecycle.Fire(evt);

                        return;
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Exception in SuiteStarted \"{0}\"", testName), e);
            }
        }

        public override void SuiteFinished(TestResult result)
        {
            try
            {
                _lifecycle.Fire(new TestSuiteFinishedEvent((string) SuiteStorage[result.FullName]));
                SuiteStorage.Remove(result.FullName);
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Exception in SuiteFinished \"{0}\"", result), e);
            }
        }

        public override void TestOutput(TestOutput testOutput)
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
            if (!WriteOutputToAttachmentFlag) return;

            if (_stdOut.Length > 0)
            {
                _lifecycle.Fire(new MakeAttachmentEvent(Encoding.UTF8.GetBytes(_stdOut.ToString()), "StdOut",
                    "text/plain"));
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
                _lifecycle.Fire(new MakeAttachmentEvent(Encoding.UTF8.GetBytes(_stdErr.ToString()), "StdErr",
                    "text/plain"));
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