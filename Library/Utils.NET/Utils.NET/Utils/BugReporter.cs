using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Modules;

namespace Utils.NET.Utils
{
    public static class BugReporter
    {
        static BugReporter()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        }

        private static event Action<string> sendBugAction;

        public static void Setup(Action<string> sendBugAction)
        {
            BugReporter.sendBugAction += sendBugAction;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            sendBugAction?.Invoke(args.ExceptionObject.ToString());
        }
    }
}
