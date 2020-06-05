using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    enum LogType
    {
        DEBUG, INFO, ERROR, WARNING
    }

    public class Logger
    {
        private IProgress<String> mProgress;

        private String filePath()
        {
            return "log_" + Settings.Player.playerName + ".txt";
        }

        public Logger(IProgress<String> aProgress)
        {
            mProgress = aProgress;
        }

        public void debug(
            String aMessage,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            sendLog(aMessage, LogType.DEBUG, callerName, callerFilePath, callerLineNumber);
        }

        public void info(
            String aMessage,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            sendLog(aMessage, LogType.INFO, callerName, callerFilePath, callerLineNumber);
        }

        public void error(
            String aMessage,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            sendLog(aMessage, LogType.ERROR, callerName, callerFilePath, callerLineNumber);
        }

        public void error(
            Exception exception,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            sendLog(exception.Message, LogType.ERROR, callerName, callerFilePath, callerLineNumber);
            sendLog(exception.StackTrace, LogType.ERROR, callerName, callerFilePath, callerLineNumber);
        }


        private void sendLog(
            String aMessage,
            LogType aLogType,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = -1
            )
        {

            // Build output string
            StringBuilder sb = new StringBuilder();
            sb.Append(aLogType);
            sb.Append(" :: ");
            sb.Append(Path.GetFileNameWithoutExtension(callerFilePath));
            sb.Append(".");
            sb.Append(callerName);
            sb.Append("(");
            sb.Append(callerLineNumber);
            sb.Append(") :: ");
            sb.Append(aMessage);


            String lMessage = sb.ToString();

            using (StreamWriter sw = new StreamWriter(filePath(), true))
            {
                sw.WriteLine(lMessage);
            }

            switch (aLogType)
            {
                case LogType.DEBUG:
                    if (Settings.DEBUG)
                    {
                        Console.Out.WriteLine(lMessage);
                        mProgress.Report(lMessage);
                    }
                    break;
                case LogType.WARNING:
                case LogType.ERROR:
                case LogType.INFO:
                default:
                    Console.Out.WriteLine(lMessage);
                    mProgress.Report(lMessage);
                    break;
            }
        }
    }
}
