using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Drawing;
using System.Diagnostics;

namespace NodBot.Code
{
    /// <summary>
    /// This is an enumerator defining the various combat states of the bot.
    /// 
    /// </summary>
    enum CombatState
    {
        BOT_START = 99,
        WAIT = 0,
        WAIT_2 = 1,
        ATTACK = 2,
        END = 3,
        INIT = 4,
    }

    public class Game
    {
        private bool PLAY;
        private CombatState mCombatState;
        private Input mInputController;
        private Logger mLogger;
        private ImageAnalyze mImageAnalyze;

        private IProgress<int> mProgressKillCount, mProgressChestCount;

        private int mKillCount = 0, mExceptionCounter = 0;
        private Stopwatch sw;

        public static IntPtr GAME { get; set; }

        public Game(Logger aLogger, IProgress<int> aProgressKill, IProgress<int> aProgressChest)
        {
            mLogger = aLogger;
            mInputController = new Input("Nodiatis", mLogger); // init the input controller
            mImageAnalyze = new ImageAnalyze(mLogger);
            GAME = mInputController.GAME;
            sw = new Stopwatch();

            mProgressKillCount = aProgressKill;
            mProgressChestCount = aProgressChest;
        }

        /// <summary>
        /// Toggles PLAY to true, and starts game loop
        /// </summary>
        public async Task StartAsync()
        {
            PLAY = true;

            mCombatState = CombatState.BOT_START;

            while (PLAY)
            {
                try
                {
                    if (mCombatState >= CombatState.WAIT && mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
                    {
                        await combatEndAsync();
                    }
                    else if (mCombatState >= CombatState.END && mImageAnalyze.ContainsMatch(NodImages.Dust, NodImages.CurrentSS))
                    {
                        await combatInitAsync();
                    }
                    else if (mCombatState >= CombatState.INIT && mImageAnalyze.ContainsMatch(NodImages.InCombat, NodImages.CurrentSS))
                    {
                        await combatStartAsync();
                    }
                    else
                    {
                        await combatBetweenStateAsync();
                    }
                }catch(Exception ex)
                {
                    mLogger.sendMessage("Game.Start() :: " + ex.ToString(), LogType.WARNING);
                    mLogger.sendMessage("Game.Start() :: " + ex.StackTrace, LogType.WARNING);
                }
            }
        }

        /// <summary>
        /// This function is called to update the exception counter, the current version may run into 
        /// trouble with the libraries used if they toggle start/stop too quickly, this will stop the bot
        /// from freezing up if this occurs.
        /// 
        /// </summary>
        private void updateExceptionFailSafe()
        {
            if (mExceptionCounter == 0 || sw.ElapsedMilliseconds > 15000)
            {
                mExceptionCounter++;
                sw.Reset();
                sw.Start();
            }else if(mExceptionCounter >= 10)
            {
                Stop();
                sw.Stop();
            }
            else
            {
                mExceptionCounter++;
            }
        }

        private async Task combatInitAsync()
        {
            mCombatState = CombatState.INIT;

            mLogger.sendMessage("Starting Combat", LogType.INFO);
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.START_FIGHT);
            await delay(100 + generateOffset(100));

        }

        private async Task combatStartAsync()
        {
            // If bot is started and already in combat, skip starting combat
            if (mCombatState == CombatState.BOT_START)
            {
                mCombatState = CombatState.ATTACK;
                mLogger.sendMessage("Already in combat", LogType.DEBUG);
                return;
            }

            mLogger.sendMessage("Starting Attack", LogType.INFO);
            await delay(400 + generateOffset(500));

            // start auto attack [A/S]
            if (Settings.MELEE)
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.AUTO_ATTACK);
            }
            else
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.AUTO_SHOOT);
            }

            await delay(2000 + generateOffset(2000));

            // start class ability [D/F]
            if (Settings.CA_PRIMARTY)
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.CA_PRIMARY);
            }
            else
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.CA_SECONDARY);
            }

            // long wait at start of combat
            await delay (20000 + generateOffset(15000));

            mCombatState = CombatState.ATTACK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatEndAsync()
        {
            // In case bot is started at end of combat, it will still attempt to loot
            if (mCombatState == CombatState.BOT_START)
                mCombatState = CombatState.WAIT_2;

            mLogger.sendMessage("Ending combat", LogType.INFO);

            if (!Settings.PILGRIMAGE)
            {
                if (mCombatState <= CombatState.END)
                {
                    // Need to figure out how to make call on ui thread
                    // mListener.updateKillCount();
                    mKillCount++; //increment kill count
                    mProgressKillCount.Report(1);

                    // loot trophies
                    mLogger.sendMessage("Lotting trophies", LogType.INFO);
                    mInputController.sendKeyboardClick(Input.Keyboard_Actions.LOOT);
                    await delay(2000 + generateOffset(2000));

                    // loot chest
                    if (Settings.CHESTS)
                    {
                        mLogger.sendMessage("Starting search for chests.", LogType.INFO);
                        Point? coord = mImageAnalyze.FindChestCoord();
                        if (coord != null)
                        {
                            mInputController.sendLeftMouseClick(coord.Value.X, coord.Value.Y);
                            mProgressChestCount.Report(1); // update ui chest counter
                        }
                        await delay (2000 + generateOffset(1000));
                    }
                }
                else
                {
                    mLogger.sendMessage("Already looted, trying to exit combat again..", LogType.DEBUG);
                    await delay (250 + generateOffset(250));
                }
            }
            else
            {
                mLogger.sendMessage("Pilgrimage Active, skipping trophies and chests.", LogType.DEBUG);
                await delay (250 + generateOffset(250));
            }

            // exit combat
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.EXIT);

            //update combat state
            mCombatState = CombatState.END;

            // Check if we should take break after some random range of kills
            await delay(takeBreak() + generateOffset(1000));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatBetweenStateAsync()
        {

                if (mCombatState == CombatState.WAIT)
                {
                    mCombatState = CombatState.WAIT_2;
                    mLogger.sendMessage("Waiting for known combat state.", LogType.INFO);
                }

                await delay(3000);
            
        }

        private int takeBreak()
        {
            int lBreakTime = 500;
            int lRandomRoundSize = new Random().Next(15, 40);
            if(mKillCount > lRandomRoundSize)
            {
                lBreakTime = new Random().Next(800, 4500);
                mKillCount = 0;
                mLogger.sendMessage("Taking short break: " + lBreakTime + "s", LogType.INFO);
            }

            return lBreakTime;
        }

        /// <summary>
        /// Toggles PLAY to false, and lets the game loop fall out and stop.
        /// </summary>
        public void Stop()
        {
            PLAY = false;
        }

        private async Task delay(int aTime)
        {
            await Task.Delay(aTime);
        }


        /// <summary>
        /// This function generates a delay offset to build a variance in the pauses of the application
        /// based on how long the original delay is set to.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        private int generateOffset(int delay)
        {
            int offset;
            if(delay <= 1000)
            {
                offset = new Random().Next(50, 250);
            }
            else if(delay <= 2000)
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
