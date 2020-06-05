using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    class TimeService
    {
        private Logger mLogger;

        public TimeService(Logger logger)
        {
            mLogger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum OffsetLength
        {
            Short, Medium, Long
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        public void delay(int delay)
        {
            Task.Delay(delay).Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="length"></param>
        public void delay(int delay, OffsetLength length)
        {
            Task.Delay(delay+ generateOffset(length)).Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public void delay(OffsetLength length)
        {
            delay(generateOffset(length));
        }

        /// <summary>
        /// This function decides if the bot should take a break based on a random kill count 
        /// and your current kill count.
        /// 
        /// </summary>
        /// <returns></returns>
        public bool takeBreak(int killCount)
        {
            if (true) return true; // Disable breaking

            int breakTime = 20000; // 20 seconds, TODO :: place break timer in settings file?
            int lRandomRoundSize = new Random().Next(30, 40); // TODO :: place range in settings file?
            if (killCount > lRandomRoundSize)
            {
                breakTime += new Random().Next(800, 4500);
                mLogger.info("Taking short break: " + breakTime + "s");

                delay(breakTime);
                return true;
            }

            return false;
        }
        /// <summary>
        /// This function generates a delay offset to build a variance in the pauses of the application
        /// based on how long the original delay is set to.
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private int generateOffset(OffsetLength length)
        {
            int offset;
            if (length == OffsetLength.Short)
            {
                offset = new Random().Next(50, 250);
            }
            else if (length == OffsetLength.Medium)
            {
                offset = new Random().Next(50, 250);
            }
            else
            {
                offset = new Random().Next(0, 7000);
            }

            return offset;
        }
    }
}
