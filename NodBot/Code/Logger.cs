using System;
using System.Collections.Generic;
using System.Linq;
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

        private UILogMessenger mLogUI;

        public Logger(UILogMessenger aLog)
        {
            mLogUI = aLog;
        }

        public void sendMessage(String aMessage, LogType aLogType)
        {
            switch (aLogType)
            {
                case LogType.DEBUG:
                    sendDebug(aMessage);
                    break;
                case LogType.WARNING:
                    sendInfo(aMessage);
                    break;
                case LogType.ERROR:
                    sendError(aMessage);
                    break;
                default:
                    sendInfo(aMessage);
                    break;
            }
        }

        private void sendDebug(String aMessage)
        {
            if (Settings.DEBUG)
            {
                Console.Out.WriteLine(DEBUG + " :: " + aMessage);
                mLogUI.updateLog(DEBUG + " :: " + aMessage);
            }
        }

        private void sendInfo(String aMessage)
        {
            Console.Out.WriteLine(INFO + " :: " + aMessage);
            mLogUI.updateLog(INFO + " :: " + aMessage);

        }

        private void sendError(String aMessage)
        {
            Console.Out.WriteLine(ERROR + " :: " + aMessage);
            mLogUI.updateLog(ERROR + " :: " + aMessage);

        }

        private void sendWarning(String aMessage)
        {
            Console.Out.WriteLine(WARNING + " :: " + aMessage);
            mLogUI.updateLog(WARNING + " :: " + aMessage);

        }
    }
}
