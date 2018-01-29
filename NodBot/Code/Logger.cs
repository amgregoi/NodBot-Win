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
        private const String DEBUG =   "DEBUG  ";
        private const String INFO =    "INFO   ";
        private const String ERROR =   "ERROR  ";
        private const String WARNING = "WARNING";

        private IProgress<String> mProgress;

        private StreamWriter sw;

        public Logger(IProgress<String> aProgress)
        {
            sw = new StreamWriter(new FileStream("log.txt", FileMode.Truncate, FileAccess.Write, FileShare.Read));
            sw.AutoFlush = true;

            mProgress = aProgress;
            
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
            sw.WriteLine(lMessage); //write debug formatted output to log file

            if(aLogType != LogType.DEBUG)
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
