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
    public enum LogType
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


        public void sendLog(
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
        }
        public void sendMessage(
        String aMessage,
        LogType aLogType,
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = -1
        )
        {
            // Build output string
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            sb.Append(" :: ");
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

            if (aLogType != LogType.DEBUG)
            {
                sb.Clear();
                sb.Append(aLogType);
                sb.Append(" :: ");
                sb.Append(aMessage);
                lMessage = sb.ToString();
            }


            switch (aLogType)
            {
                case LogType.DEBUG:
                    sendDebug(lMessage);
                    break;
                case LogType.WARNING:
                    sendInfo(lMessage);
                    break;
                case LogType.ERROR:
                    sendError(lMessage);
                    break;
                default:
                    sendInfo(lMessage);
                    break;
            }
        }

        private void sendDebug(String aMessage)
        {
            if (Settings.DEBUG)
            {
                Console.Out.WriteLine(aMessage);
                mProgress.Report(aMessage);
            }
        }

        private void sendInfo(String aMessage)
        {

            Console.Out.WriteLine(aMessage);
            mProgress.Report(aMessage);

        }

        private void sendError(String aMessage)
        {

            Console.Out.WriteLine(aMessage);
            mProgress.Report(aMessage);

        }

        private void sendWarning(String aMessage)
        {
            Console.Out.WriteLine(aMessage);
            mProgress.Report(aMessage);
        }
    }
}
