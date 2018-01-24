using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Drawing;

namespace NodBot.Code
{
    public class Game
    {
        private bool PLAY, FIGHTING_STATE, KNOWN_STATE;
        private Input mInputController;
        private Logger mLogger;
        private ImageAnalyze mImageAnalyze;
        private UIKillCounter mListener;

        private int mKillCount = 0;

        public static IntPtr GAME { get; set; }

        public Game(UIKillCounter aListener, Logger aLogger)
        {
            mListener = aListener;
            mLogger = aLogger;
            mInputController = new Input("Nodiatis", mLogger); // init the input controller
            mImageAnalyze = new ImageAnalyze(mLogger);
            GAME = mInputController.GAME;
        }

        public void moveMouse(Point p)
        {
            mInputController.moveMouse(p.X, p.Y);
        }

        /// <summary>
        /// Toggles PLAY to true, and starts game loop
        /// </summary>
        public async Task StartAsync()
        {
            FIGHTING_STATE = mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS);
            PLAY = true;
            KNOWN_STATE = false;

            while (PLAY)
            {
                try
                {
                    if (FIGHTING_STATE && mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
                    {
                        await combatEndAsync();
                    }
                    else if (!FIGHTING_STATE && mImageAnalyze.ContainsMatch(NodImages.Handbook, NodImages.CurrentSS))
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

        private async Task combatStartAsync()
        {
            mLogger.sendMessage("Starting combat", LogType.INFO);
            KNOWN_STATE = false;

            do
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.START_FIGHT);
                await Task.Delay(100 + generateOffset(100));
            }
            while (mImageAnalyze.ContainsMatch(NodImages.Handbook, NodImages.CurrentSS));
            await Task.Delay(400 + generateOffset(500));

            // start auto attack [A/S]
            if (Settings.MELEE)
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.AUTO_ATTACK);
            }
            else
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.AUTO_SHOOT);
            }

            await Task.Delay(2000 + generateOffset(2000));

            // start class ability [D/F]
            if (Settings.CA_PRIMARTY)
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.CA_PRIMARY);
            }
            else
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.CA_SECONDARY);
            }

            FIGHTING_STATE = true;
            await Task.Delay(5000 + generateOffset(25000));

            while (!mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
            {
                await Task.Delay(2000 + generateOffset(2000)); // busy wait till end of combat
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatEndAsync()
        {
            mLogger.sendMessage("Ending combat", LogType.INFO);
            KNOWN_STATE = false;

            if (!Settings.PILGRIMAGE)
            {
                // loot trophies
                mLogger.sendMessage("Lotting trophies", LogType.INFO);
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.LOOT);
                await Task.Delay(2000 + generateOffset(2000));

                // loot chest
                if (Settings.CHESTS)
                {
                    mLogger.sendMessage("Starting search for chests.", LogType.INFO);
                    Point? coord = mImageAnalyze.FindChestCoord();
                    if (coord != null) mInputController.sendLeftMouseClick(coord.Value.X, coord.Value.Y);
                    await Task.Delay(2000 + generateOffset(1000));
                }
            }
            else
            {
                mLogger.sendMessage("Pilgrimage Active, skipping trophies and chests.", LogType.DEBUG);
                await Task.Delay(250 + generateOffset(250));
            }

            // exit combat
            do
            {
                mInputController.sendKeyboardClick(Input.Keyboard_Actions.EXIT);
                await Task.Delay(1000 + generateOffset(1000));
            }
            while (!mImageAnalyze.ContainsMatch(NodImages.Handbook, NodImages.CurrentSS));

            FIGHTING_STATE = false;

            // Need to figure out how to make call on ui thread
            // mListener.updateKillCount();
            mKillCount++;

            // Check if we should take break after some random range of kills
            await Task.Delay(takeBreak() + generateOffset(1000));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatBetweenStateAsync()
        {
            if (!KNOWN_STATE)
            {
                KNOWN_STATE = true;
                mLogger.sendMessage("Waiting for known combat state.", LogType.INFO);
            }

            await Task.Delay(2000);

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
                offset = new Random().Next(0, 10000);
            }

            return offset;
        }

    }
}
