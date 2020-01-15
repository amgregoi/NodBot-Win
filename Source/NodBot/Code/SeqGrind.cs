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
/*
using OpenCvSharp;
using OpenCvSharp.Util;
static void RunTemplateMatch(string reference, string template)
{
    using (Mat refMat = new Mat(reference))
    using (Mat tplMat = new Mat(template))
    using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
    {
        //Convert input images to gray
        Mat gref = refMat.CvtColor(ColorConversionCodes.BGR2GRAY);
        Mat gtpl = tplMat.CvtColor(ColorConversionCodes.BGR2GRAY);

        Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
        Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);

        while (true)
        {
            double minval, maxval, threshold = 0.8;
            Point minloc, maxloc;
            Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

            if (maxval >= threshold)
            {
                //Setup the rectangle to draw
                Rect r = new Rect(new Point(maxloc.X, maxloc.Y), new Size(tplMat.Width, tplMat.Height));

                //Draw a rectangle of the matching area
                Cv2.Rectangle(refMat, r, Scalar.LimeGreen, 2);

                //Fill in the res Mat so you don't find the same area again in the MinMaxLoc
                Rect outRect;
                Cv2.FloodFill(res, maxloc, new Scalar(0), out outRect, new Scalar(0.1), new Scalar(1.0));
            }
            else
                break;
        }

        Cv2.ImShow("Matches", refMat);
        Cv2.WaitKey();
    }
}*/


// OR
/* NOTE: This one seems more promising
 using (Image<Bgr, byte> imgSrc = BaseImage.Copy())
    {
        while (true)
        {
           //updated and changed TemplateMatchingType- CcoeffNormed.
            using (Image<Gray, float> result = imgSrc.MatchTemplate(SubImage, TemplateMatchingType.CcoeffNormed)) 
            {
                CvInvoke.Threshold(result, result, 0.7, 1, ThresholdType.ToZero);
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                if (maxValues[0] > Threashold)
                {
                    Rectangle match = new Rectangle(maxLocations[0], SubImage.Size);
                    imgSrc.Draw(match, new Bgr(Color.Blue), -1);
                    rectangles.Add(match);
                }
                else
                {
                    break;
                }
            }
        }
    }
 */
namespace NodBot.Code
{

    public class SeqGrind : SeqBase
    {
        private IProgress<int> mProgressKillCount, mProgressChestCount;

        private int mKillCount = 0;
        private Stopwatch sw;
        GemSlot currentGemSlot = GemSlot.Five;

        SeqArena mArena;

        enum GemSlot
        {
            One, Two, Three, Four, Five, Six
        }

        /// <summary>
        /// Constructor for the game Grinding Sequence.
        /// 
        /// </summary>
        /// <param name="aLogger"></param>
        /// <param name="aProgressKill"></param>
        /// <param name="aProgressChest"></param>
        public SeqGrind(Logger aLogger, IProgress<int> aProgressKill, IProgress<int> aProgressChest) : base(aLogger)
        {
            sw = new Stopwatch();
            mArena = new SeqArena(mLogger);

            mProgressKillCount = aProgressKill;
            mProgressChestCount = aProgressChest;
        }

        /// <summary>
        /// Toggles PLAY to true, and starts game loop
        /// 
        /// </summary>
        public override async Task Start(CancellationToken aCt)
        {

            mCombatState = SequenceState.BOT_START;
            bool WAITING_FOR_ARENA = false;
            while (true)
            {
                try
                {
                    aCt.ThrowIfCancellationRequested();

                    if (Settings.ARENA && !WAITING_FOR_ARENA)
                    {
                        await mArena.EnterQueue();
                        WAITING_FOR_ARENA = true;
                    }

                    var dialog = mImageAnalyze.FindMatchTemplate(NodImages.CurrentSS, NodImages.X);
                    if (dialog != null)
                    {
                        mInput.ClickOnPoint(dialog.Value.X, dialog.Value.Y, true);
                        continue;
                    }

                    if (mCombatState >= SequenceState.WAIT && mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
                    {
                        await combatEndAsync();
                    }
                    else if (mCombatState >= SequenceState.ATTACK && mImageAnalyze.FindMatchTemplate(NodImages.CurrentSS, NodImages.NeutralSS) != null)
                    {
                        await combatInitAsync();
                    }
                    else if (mCombatState >= SequenceState.INIT && mImageAnalyze.ContainsMatch(NodImages.InCombat, NodImages.CurrentSS))
                    {
                        await combatStartAsync();
                    }
                    else if (mImageAnalyze.ContainsMatch(NodImages.Arena, NodImages.CurrentSS))
                    {
                        await mArena.StartArenaCombat();
                        WAITING_FOR_ARENA = false;
                    }
                    else
                    {
                        await combatBetweenStateAsync();
                    }
                } catch (Exception ex)
                {
                    if (ex is OperationCanceledException) break;

                    mLogger.sendLog(ex.StackTrace, LogType.WARNING);
                }
            }
        }

        private async Task CombatArenaStart()
        {
            await delay(13000); // wait 13 seconds for count down;
            mInput.SettingsAttack();
        }

        /// <summary>
        /// This function initiates combat from the over world map.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatInitAsync()
        {
            mCombatState = SequenceState.INIT;

            mLogger.sendMessage("Starting Combat", LogType.INFO);
            mInput.InitiateFight();
            await delay(1500);

            //increment kill count
            mKillCount++;
            mProgressKillCount.Report(1);
        }

        /// <summary>
        /// This function starts attacking and uses the players class ability, both specified
        /// by the UI settings options.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatStartAsync()
        {
            // If bot is started and already in combat, skip starting combat
            if (mCombatState == SequenceState.BOT_START)
            {
                mCombatState = SequenceState.ATTACK;
                mLogger.sendMessage("Already in combat", LogType.DEBUG);
                return;
            }

            mLogger.sendMessage("Starting Attack", LogType.INFO);
            await delay(generateOffset(100));

            // start auto attack [A/S]
            if (Settings.Player.isMelee)
            {
                // TODO :: Verify  this autoshoot function works
                if (Settings.Player.startCombatRangeSwapMelee)
                {
                    mInput.AutoShoot();
                    await delay(2000 + generateOffset(500));
                }
                mInput.AutoAttack();
            }
            else
            {
                mInput.AutoShoot();
            }

            await delay(1500 + generateOffset(2000));

            // start class ability [D/F]
            if (Settings.Player.usePrimaryClassAbility)
            {
                mInput.PrimaryClassAbility();
            }
            else if (Settings.Player.useSecondaryClassAbility)
            {
                mInput.SecondaryClassAbility();
            }

            // long wait at start of combat
            //await delay (20000 + generateOffset(15000));

            mCombatState = SequenceState.ATTACK;
        }

        /// <summary>
        /// This function loots trophies, scans for chests, and exits combat.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatEndAsync()
        {
            // In case bot is started at end of combat, it will still attempt to loot
            if (mCombatState == SequenceState.BOT_START)
                mCombatState = SequenceState.WAIT_2;

            mLogger.sendMessage("Ending combat", LogType.INFO);

            if (!Settings.PILGRIMAGE)
        {
                // loot trophies
                if (!Settings.BOSSING) // SKIP loot if bossing
                {
                    mLogger.sendMessage("Lotting trophies", LogType.INFO);
                    mInput.LootTrophies();
                }
                else mLogger.sendMessage("Skipped looting trophies?", LogType.INFO);

                await delay(2000 + generateOffset(2000));

                // loot chest
                if (Settings.CHESTS)
                {
                    mLogger.sendMessage("Starting search for chests.", LogType.INFO);
                    Point? coord = mImageAnalyze.FindChestCoord();
                    if (coord != null)
                    {
                        mInput.ClickOnPoint(coord.Value.X, coord.Value.Y, true);
                        mProgressChestCount.Report(1); // update ui chest counter
                    }
                    await delay(2000 + generateOffset(1000));
                }
                else mLogger.sendMessage("Skipped chests?", LogType.INFO);
            }
            else
            {
                mLogger.sendMessage("Pilgrimage Active, skipping trophies and chests.", LogType.DEBUG);
                await delay(250 + generateOffset(250));
            }

            // exit combat
            mInput.Exit();

            //update combat state
            mCombatState = SequenceState.END;

            // Check if we should take break after some random range of kills
            await delay(takeBreak() + generateOffset(1000));
        }

        /// <summary>
        /// This function is called between combat states, and delays the task 3 seconds.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatBetweenStateAsync()
        {
            // If bot is started and no known state is found, we are stuck in the
            // over world without the Dust Collecting icon, initiate first combat
            // to keep bot state alive.
            if (mCombatState == SequenceState.BOT_START)
            {
                mLogger.sendMessage("Dust button not found after starting bot.", LogType.INFO);
                await combatInitAsync();
                return;
            }

            if (mCombatState > SequenceState.WAIT)
            {
                mLogger.sendMessage("Waiting for known combat state.", LogType.DEBUG);
            }

            if (Settings.Player.useMagic)
            {
                useGemSlot();
                UpdateNextGemSlot();
            }
            await delay(3000);
        }

        /// <summary>
        /// This function decides if the bot should take a break based on a random kill count 
        /// and your current kill count.
        /// 
        /// </summary>
        /// <returns></returns>
        private int takeBreak()
        {
            int lBreakTime = 500;
            int lRandomRoundSize = new Random().Next(15, 40);
            if (mKillCount > lRandomRoundSize)
            {
                lBreakTime = new Random().Next(800, 4500);
                mKillCount = 0;
                mLogger.sendMessage("Taking short break: " + lBreakTime + "s", LogType.INFO);
            }

            return lBreakTime;
        }

        /// <summary>
        /// This function generates a delay offset to build a variance in the pauses of the application
        /// based on how long the original delay is set to.
        /// 
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


        private void useGemSlot()
        {
            switch (currentGemSlot)
            {
                case GemSlot.One:
                    mInput.GemSlot1();
                    break;
                case GemSlot.Two:
                    mInput.GemSlot2();
                    break;
                case GemSlot.Three:
                    mInput.GemSlot3();
                    break;
                case GemSlot.Four:
                    mInput.GemSlot4();
                    break;
                case GemSlot.Five:
                    mInput.GemSlot5();
                    break;
                case GemSlot.Six:
                    mInput.GemSlot6();
                    break;
            }
        }

        private void UpdateNextGemSlot()
        {
            if (Settings.Player.magic.cycleAllGemSlots)
            {
                switch (currentGemSlot)
                {
                    case GemSlot.One:
                        currentGemSlot = GemSlot.Two;
                        break;
                    case GemSlot.Two:
                        currentGemSlot = GemSlot.Three;
                        break;
                    case GemSlot.Three:
                        currentGemSlot = GemSlot.Four;
                        break;
                    case GemSlot.Four:
                        currentGemSlot = GemSlot.Five;
                        break;
                    case GemSlot.Five:
                        if (!Settings.Player.magic.cycleAllGemSlots) currentGemSlot = GemSlot.Five;
                        else if (!Settings.Player.magic.usesStaff) currentGemSlot = GemSlot.Six;
                        else if (Settings.Player.magic.isPremium) currentGemSlot = GemSlot.One;
                        else if (Settings.Player.magic.isStandard) currentGemSlot = GemSlot.Two;
                        else currentGemSlot = GemSlot.Three;
                        break;
                    case GemSlot.Six:
                        if (Settings.Player.magic.isPremium) currentGemSlot = GemSlot.One;
                        else if (Settings.Player.magic.isStandard) currentGemSlot = GemSlot.Two;
                        else currentGemSlot = GemSlot.Three;
                        break;
                }
            }
        }
    }
}
