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
using System.Windows.Forms;

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
        private SeqMining miningService;
        private SeqGardening gardenService;
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
            miningService = new SeqMining(tokenSource, logger);
            gardenService = new SeqGardening(tokenSource, logger);
        }

        /// <summary>
        /// Toggles PLAY to true, and starts game loop
        /// 
        /// </summary>
        public override async Task Start()
        {

            combatState = SequenceState.BOT_START;

            var defaultImages = new List<string>() { NodImages.X, NodImages.X2, NodImages.ButtonNo};

            while (true)
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    imageService.CaptureScreen();

                    var result = imageService.FindTemplateMatch(defaultImages, screenSection: ScreenSection.Game);
                    var forceContinue = false;

                    result.ForEach(dialogItem =>
                    {
                        if (dialogItem != null)
                        {
                            nodInputService.ClickOnPoint(dialogItem.X, dialogItem.Y, true);
                            forceContinue = true;
                        }
                    });

                    if (forceContinue) continue;
                    

                    if (combatState >= SequenceState.WAIT && imageService.ContainsMatch(NodImages.Exit, screenSection:ScreenSection.Game))
                    {
                        // Wait for inventory service to finish running
                        while (inventoryService.isRunning) Task.Delay(500).Wait();

                        grindService.EndCombat().Wait();
                        if (Settings.MANAGE_INVENTORY && inventoryService.IsStorageEmpty) token.ThrowIfCancellationRequested();
                        Task.Delay(1000).Wait();
                    }
                    else if (combatState >= SequenceState.ATTACK && imageService.ContainsTemplateMatch(NodImages.NeutralSS, screenSection: ScreenSection.Game))
                    {

                        if (Settings.RESOURCE_MINING && imageService.ContainsTemplateMatch(NodImages.MiningIcon, ScreenSection.Game))
                        {
                            miningService.Start().Wait();
                            timeService.delay(1000);
                        }

                        if (Settings.RESOURCE_GARDEN && imageService.ContainsTemplateMatch(NodImages.GardeningIcon, ScreenSection.Game))
                        {
                            gardenService.Start().Wait();
                            timeService.delay(1000);
                        }

                        token.ThrowIfCancellationRequested();

                        // TODO :: Make resource waiting a property, off by default atm
                        if (Settings.WAIT_FOR_RESOURCES && !imageService.ContainsTemplateMatch(NodImages.PlayerResourceMinimum, screenSection: ScreenSection.Game))
                        {
                            logger.info("Waiting for resources to regen");
                            timeService.delay(3000);
                            continue;
                        }

                        token.ThrowIfCancellationRequested();

                        if (Settings.ARENA && !imageService.ContainsTemplateMatch(NodImages.ArenaVerify, ScreenSection.Game))
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                arenaService.EnterQueue().Wait();

                                // If we find arena queue icon, exit loop
                                timeService.delay(250);
                                if (imageService.ContainsTemplateMatch(NodImages.ArenaVerify, ScreenSection.Game))
                                {
                                    isInArenaQueue = true;
                                    break;
                                }
                            }
                        }

                        token.ThrowIfCancellationRequested();

                        grindService.enterCombat().Wait();
                        Task.Delay(1500).Wait();
                    }
                    else if (Settings.ARENA && imageService.ContainsTemplateMatch(NodImages.Arena, screenSection: ScreenSection.Game))
                    {
                        if (!isInArenaQueue) continue; // Already in arena combat, don't restart count-down

                        for (int i = 13; i >= 0; i--)
                        {
                            logger.info("Starting arena in ~" + i + "secs");
                            Task.Delay(1000).Wait();
                        }

                        isInArenaQueue = false;

                        arenaService.StartCombat().Wait();
                    }
                    else if (combatState >= SequenceState.INIT && imageService.ContainsMatch(NodImages.InCombat, screenSection: ScreenSection.Game))
                    {
                        grindService.startCombat().Wait();
                    }
                    else
                    {
                        await CombatBetweenStateAsync();
                    }
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException) break;
                    logger.error(ex);
                }
            }
        }

        /// <summary>
        /// This function is called between combat states, and delays the task 3 seconds.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task CombatBetweenStateAsync()
        {
            if (combatState > SequenceState.WAIT)
            {
                logger.debug("Currently in combat.");
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
