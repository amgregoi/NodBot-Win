using Emgu.CV.CvEnum;
using NodBot.Code.Enums;
using NodBot.Code.Model;
using NodBot.Code.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NodBot.Code
{
    public class SeqMining : SeqBase
    {
        public SeqMining(CancellationTokenSource ct, Logger aLogger) : base(ct, aLogger) { }


        public override async Task Start()
        {
            combatState = SequenceState.RESOURCING;
            testMining();

        }

        private void testMining()
        {
            var iconPoint = imageService.FindTemplateMatch(NodImages.MiningIcon, screenSection: ScreenSection.Game);

            if (iconPoint != null)
            {
                nodInputService.ClickOnPoint(iconPoint.X, iconPoint.Y, true);
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
            Task.Delay(1000).Wait(); // Wait for mining to start
            nodInputService.MoveCursorTo(20, 50);
            Task.Delay(500);

            var isMining = imageService.ContainsMatch(NodImages.Exit, screenSection: ScreenSection.Game);

            string[] images = new string[] { NodImages.Rock1, NodImages.Rock2, NodImages.Rock3, NodImages.Rock4 };
            int imageIndex = 0;

            while (isMining)
            {
                UIPoint point = imageService.FindTemplateMatch(templateImage: images[imageIndex], screenSection: ScreenSection.MiniGame, threshold: 0.98, matchType: TemplateMatchingType.CcorrNormed);

                if (point == null)
                {
                    isMining = imageService.ContainsMatch(NodImages.Exit, screenSection: ScreenSection.Game);

                    imageIndex++;
                    imageIndex %= images.Length;
                    continue;
                }
                else
                {
                    for (int i = -2; i < 2; i++)
                    {
                        int newX = point.X + (i * 60);
                        int newX2 = point.X + (i * 30);

                        if (newX > 800) newX = 800;
                        else if (newX < 0) newX = 10;

                        nodInputService.ClickOnPoint(newX, point.Y, true);
                        Task.Delay(75).Wait();
                        nodInputService.ClickOnPoint(newX2, point.Y, true);
                        Task.Delay(75).Wait();
                    }
                }

                Task.Delay(500).Wait();
                isMining = imageService.ContainsMatch(NodImages.Exit, screenSection: ScreenSection.Game);
            }

            MoveOreToInventory();
        }

        private void MoveOreToInventory()
        {
            UIPoint emptyInventory = inventoryService.GetFirstEmptyInventorySpace();
            if (emptyInventory != null) nodInputService.ClickOnPoint(emptyInventory.X, emptyInventory.Y, true);
            else
            {
                var emptyStorage = inventoryService.GetFirstEmptyStorageSpace();
                if (emptyStorage != null) nodInputService.ClickOnPoint(emptyInventory.X, emptyInventory.Y, true);
                else tokenSource.Cancel();
            }

            logger.sendMessage("Done with rocks..", LogType.ERROR);

        }
    }
}