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
using NodBot.Code.Services;
using NodBot.Code.Enums;

namespace NodBot.Code
{

    public class SeqGrind : SeqBase, GrindCallback
    {
        private IProgress<int> mProgressKillCount;
        private IProgress<int> mProgressChestCount;

        private GrindService grindService;
        private ArenaService arenaService;
        private MagicService magicService;
        private TimeService timeService;

        private bool isInArenaQueue = false;

        /// <summary>
        /// Constructor for the game Grinding Sequence.
        /// 
        /// </summary>
        /// <param name="aLogger"></param>
        /// <param name="aProgressKill"></param>
        /// <param name="aProgressChest"></param>
        public SeqGrind(CancellationTokenSource ct, Logger aLogger, IProgress<int> aProgressKill, IProgress<int> aProgressChest) : base(ct, aLogger)
        {
            mProgressKillCount = aProgressKill;
            mProgressChestCount = aProgressChest;

            timeService = new TimeService(logger);
            grindService = new GrindService(imageService, nodInputService, logger, this);
            arenaService = new ArenaService(imageService, nodInputService, logger, this);
            magicService = new MagicService(nodInputService);
        }

        /// <summary>
        /// Toggles PLAY to true, and starts game loop
        /// 
        /// </summary>
        public override async Task Start()
        {

            combatState = SequenceState.BOT_START;

            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    imageService.CaptureScreen();

                    var dialog = imageService.FindMatchTemplate(NodImages.CurrentSS, NodImages.X);
                    if (dialog != null)
                    {
                        nodInputService.ClickOnPoint(dialog.Value.X, dialog.Value.Y, true);
                        continue;
                    }

                    if (combatState >= SequenceState.WAIT && imageService.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
                    {
                        grindService.EndCombat().Wait();
                        if (Settings.isManagingInventory() && inventoryService.isStorageEmpty()) token.ThrowIfCancellationRequested();
                        Task.Delay(1000).Wait();
                    }
                    else if (combatState >= SequenceState.ATTACK && imageService.FindMatchTemplate(NodImages.CurrentSS, NodImages.NeutralSS) != null)
                    {

                        if (Settings.RESOURCE_MINING && imageService.FindTemplateMatchWithXConstraint(NodImages.MiningIcon, 950, true, true).Count > 0)
                        {
                            new SeqMining(tokenSource, logger).Start().Wait();
                            timeService.delay(1000);
                        }

                            // TODO :: Make resource waiting a property, off by default atm
                            if (Settings.WAIT_FOR_RESOURCES && imageService.FindMatchTemplate(NodImages.CurrentSS, NodImages.PlayerResourceMinimum) == null)
                        {
                            logger.sendLog("Waiting for resources to regen", LogType.INFO);
                            timeService.delay(3000);
                            continue;
                        }

                        if (Settings.ARENA && imageService.FindTemplateMatchWithYConstraint(NodImages.ArenaVerify, 700, false).Count == 0)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                arenaService.EnterQueue().Wait();

                                // If we find arena queue icon, exit loop
                                timeService.delay(250);
                                if (imageService.FindTemplateMatchWithYConstraint(NodImages.ArenaVerify, 700, false, true).Count > 0)
                                {
                                    isInArenaQueue = true;
                                    break;
                                }
                            }
                        }

                        grindService.enterCombat().Wait();
                        Task.Delay(1500).Wait();
                    }
                    else if (Settings.ARENA && imageService.FindMatchTemplate(NodImages.CurrentSS, NodImages.Arena) != null)
                    {
                        if (!isInArenaQueue) continue; // Already in arena combat, don't restart count-down

                        for(int i=13; i>=0; i--)
                        {
                            logger.sendMessage("Starting arena in ~" + i + "secs", LogType.INFO);
                            Task.Delay(1000).Wait();
                        }

                        isInArenaQueue = false;

                        arenaService.StartCombat().Wait();
                    }
                    else if (combatState >= SequenceState.INIT && imageService.ContainsMatch(NodImages.InCombat, NodImages.CurrentSS))
                    {
                        grindService.startCombat().Wait();
                    }
                    else
                    {
                        await combatBetweenStateAsync();
                    }
                } catch (Exception ex)
                {
                    if (ex is OperationCanceledException) break;

                    logger.sendLog(ex.StackTrace, LogType.WARNING);
                }
            }
        }

        /// <summary>
        /// This function is called between combat states, and delays the task 3 seconds.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task combatBetweenStateAsync()
        {
            if (combatState > SequenceState.WAIT)
            {
                logger.sendMessage("Currently in combat.", LogType.DEBUG);
            }

            if (Settings.Player.useMagic)
            {
                magicService.UseGemSlot();
                magicService.UpdateNextGemSlot();
            }

            timeService.delay(3000);
        }



        /***
         * 
         * GrindCallback definitions
         * 
         */

        void GrindCallback.updateKillCounter()
        {
            mProgressKillCount.Report(1);
        }

        void GrindCallback.updateChestCounter()
        {
            mProgressChestCount.Report(1);
        }
    }
}
