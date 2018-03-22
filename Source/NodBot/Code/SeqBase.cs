using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public abstract class SeqBase
    {
        protected Logger mLogger;
        protected ImageAnalyze mImageAnalyze;
        protected NodiatisInput mInput;

        protected SequenceState mCombatState;

        protected SeqBase(Logger aLogger)
        {
            mCombatState = SequenceState.BOT_START;

            mLogger = aLogger;
            mImageAnalyze = new ImageAnalyze(mLogger);
            mInput = new NodiatisInput(mLogger);
        }

        public virtual async Task Start(CancellationToken aCt)
        {
            mLogger.sendLog("Base Method NOT IMPLEMENTED", LogType.INFO);
            await Task.Yield();
        }

        /// <summary>
        /// This function delays the town walking task by the specified time (ms).
        /// 
        /// </summary>
        /// <param name="aTime"></param>
        /// <returns></returns>
        protected async Task delay(int aTime)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(aTime);
            });
        }

        /// <summary>
        /// This is an enumerator defining the various combat states of the bot.
        /// 
        /// </summary>
        protected enum SequenceState
        {
            BOT_START = 100,

            //Combat States
            WAIT = 0,
            WAIT_2 = 1,
            ATTACK = 2,
            END = 3,
            INIT = 4,

            //Arena States
            ARENA_WAIT_QUEUE = 20,
            ARENA_WAIT_COMBAT = 21,
            ARENA = 22,

            //TW States
            TOWN_WALKING = 30,
        }
    }
}
