using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class SeqArena : SeqBase
    { 

        public SeqArena(Logger aLogger) : base(aLogger)
        {

        }

        public override async Task Start(CancellationToken aCt)
        {
            mCombatState = SequenceState.ARENA;
            while (true)
            {
                aCt.ThrowIfCancellationRequested();

                if (mCombatState >= SequenceState.ARENA_WAIT_QUEUE && mImageAnalyze.ContainsMatch(NodImages.Arena, NodImages.CurrentSS))
                {
                    await StartArenaCombat();
                }
                else if (mCombatState >= SequenceState.ARENA_WAIT_COMBAT && mImageAnalyze.ContainsMatch(NodImages.Dust, NodImages.CurrentSS))
                {
                    await EnterQueue();
                }
                else
                {
                    await delay(1500); // wait 1.5s
                }
            }            
        }

        public async Task StartArenaCombat()
        {
            mLogger.sendMessage("Starting ARENA combat in 13(s)", LogType.INFO);
            await delay(14000);
            mInput.SettingsAttack();
            mCombatState = SequenceState.ARENA_WAIT_COMBAT; // waiting for arena to end

            while (!mImageAnalyze.ContainsMatch(NodImages.Dust, NodImages.CurrentSS))
            {
                await delay(1000);
            }
        }

        public async Task EnterQueue()
        {

            mCombatState = SequenceState.ARENA_WAIT_QUEUE;

            Random rand = new Random();

            int xOffset = rand.Next(500);
            int yOffset = rand.Next(150);

            int xInit = 200, yInit = 100;

            mInput.ClickOnPoint(xInit + xOffset, yInit + yOffset, false);

            await delay(1000);
            xOffset += rand.Next(20, 50);
            mInput.ClickOnPoint(xInit + xOffset + rand.Next(20, 50), yInit + yOffset + 25, true);

            await delay(1000);
            xOffset += rand.Next(20, 50);
            mInput.ClickOnPoint(xInit + xOffset + rand.Next(20, 50), yInit + yOffset + 72, true);


            await Task.Delay(25);

        }
    }
}
