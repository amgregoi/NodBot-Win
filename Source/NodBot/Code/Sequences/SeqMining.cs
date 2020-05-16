using Emgu.CV.CvEnum;
using NodBot.Code.Enums;
using NodBot.Code.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NodBot.Code
{
    public class SeqMining : SeqBase
    {
        private ArenaService arenaService;
        private TimeService timeService;

        public SeqMining(CancellationTokenSource ct, Logger aLogger) : base(ct, aLogger)
        {
            timeService = new TimeService(aLogger);
            arenaService = new ArenaService(imageService, nodInputService, logger, this);
        }

        public override async Task Start()
        {
            combatState = SequenceState.RESOURCING;
            testMining();

        }

        private void testMining()
        {
            var iconPoint = imageService.FindMatchTemplate(NodImages.CurrentSS, NodImages.MiningIcon, true);

            if (iconPoint != null)
            {
                nodInputService.ClickOnPoint(iconPoint.Value.X, iconPoint.Value.Y, true);
                startMining();
            }
            else
            {
                logger.sendMessage("super rip", LogType.ERROR);
                startMining();
            }
        }

        private void startMining()
        {
            Task.Delay(1500).Wait(); // Wait for mining to start
            var isMining = imageService.ContainsMatch(NodImages.Exit, NodImages.CurrentSS);

            string[] images = new string[] { NodImages.Rock1, NodImages.Rock2, NodImages.Rock3 };
            int imageIndex = 0;

            while (isMining)
            {
                Rectangle? match = imageService.FindTemplateMatchWithYConstraintSingle(templateImage: images[imageIndex], yConstraint: 600, lessThanY: true, updateCurrentScreen: true, threshold: 0.952, matchType: TemplateMatchingType.CcorrNormed);

                if (match == null)
                {
                    imageIndex++;
                    imageIndex %= 3;
                    continue;
                }

                var point = match.Value;
                int x = point.X + (point.Width / 2);
                int y = point.Y + (point.Height / 2);

                if (point != null)
                {
                    for (int i = -4; i < 4; i++)
                    {
                        int newX = x + (i * 30);
                        if (newX > 800) newX = 800;
                        else if (newX < 0) newX = 10;
                        nodInputService.ClickOnPoint(newX, y, true);
                        Task.Delay(75).Wait();
                        nodInputService.ClickOnPoint(newX, y, true);
                        Task.Delay(75).Wait();
                    }
                }

                Task.Delay(150).Wait();
                isMining = imageService.ContainsMatch(NodImages.Exit, NodImages.CurrentSS);
            }

            MoveOreToInventory();
        }

        private void MoveOreToInventory()
        {
            var emptyInventory = inventoryService.getFirstEmptyInventorySpace();
            if (emptyInventory != null) nodInputService.ClickOnPoint(emptyInventory.Value.X, emptyInventory.Value.Y, true);
            else
            {
                var emptyStorage = inventoryService.getFirstEmptyStorageSpace();
                if (emptyStorage != null) nodInputService.ClickOnPoint(emptyInventory.Value.X, emptyInventory.Value.Y, true);
                else tokenSource.Cancel();
            }

            logger.sendMessage("Done with rocks..", LogType.ERROR);

        }
    }
}
