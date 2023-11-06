using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TuShan.BountyHunterDream.Logger
{
    public class TLog
    {
        public static void Configure(string p_configFile)
        {
            XmlConfigurator.Configure(new FileInfo(p_configFile));
        }

        public static void Error(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateLogger(memberName, sourceFilePath, sourceLineNumber).Error(message);
        }

        public static void Error(object message, Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateLogger(memberName, sourceFilePath, sourceLineNumber).Error(message, exception);
        }

        public static void Info(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateLogger(memberName, sourceFilePath, sourceLineNumber).Info(message);
        }

        public static void Info(object message, Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateLogger(memberName, sourceFilePath, sourceLineNumber).Info(message, exception);
        }

        public static void Debug(object message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateLogger(memberName, sourceFilePath, sourceLineNumber).Debug(message);
        }

        public static void Debug(object message, Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            CreateLogger(memberName, sourceFilePath, sourceLineNumber).Debug(message, exception);
        }

        private static ILog CreateLogger(string memberName, string sourceFile, int sourceLine)
        {
            var temp = sourceFile.Split('\\');
            return LogManager.GetLogger($"{temp[temp.Length - 1]}:{sourceLine} {memberName}");
        }
    }
}
