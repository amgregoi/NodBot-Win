using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NodBot.Code.Enums;
using NodBot.Code.Model;
using NodBot.Code.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace NodBot.Code
{
    public class SeqGardening : SeqBase
    {
        public SeqGardening(CancellationTokenSource ct, Logger aLogger) : base(ct, aLogger) { }

        private static int xOff = 15, yOff = 35, width = 60, height = 60;

        UIPoint[,] points = new UIPoint[4, 4]
        {
            { UIPoint.New(new Rectangle(250+xOff,153+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,153+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,153+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,153+yOff,width, height)) },
            { UIPoint.New(new Rectangle(250+xOff,233+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,233+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,233+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,233+yOff,width, height)) },
            { UIPoint.New(new Rectangle(250+xOff,313+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,313+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,313+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,313+yOff,width, height)) },
            { UIPoint.New(new Rectangle(250+xOff,393+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,393+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,393+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,393+yOff,width, height)) },
        };

        Image<Bgr, byte>[,] icons = new Image<Bgr, byte>[4, 4]
            {
                {null, null,null,null },
                {null, null,null,null },
                {null, null,null,null },
                {null, null,null,null }
            };

        Boolean[,] isComplete = new Boolean[4, 4]
        {
            {false, false, false, false },
            {false, false, false, false },
            {false, false, false, false },
            {false, false, false, false }
        };

        public override async Task Start()
        {
            combatState = SequenceState.RESOURCING;
            var iconPoint = imageService.FindTemplateMatch(NodImages.GardeningIcon, screenSection: ScreenSection.Game);

            if (iconPoint != null)
            {
                nodInputService.ClickOnPoint(iconPoint.X, iconPoint.Y, true);
                startGarden();
            }
            else
            {
                startGarden();
            }
        }

        private async Task startGarden()
        {
            Task.Delay(500).Wait();
            while(imageService.ContainsMatch(NodImages.Exit, ScreenSection.Game))
            {
                try
                {
                    Point? firstPoint = null;
                    for (int i = 0; i < points.GetLength(0); i++)
                    {
                        for (int j = 0; j < points.GetLength(1); j++)
                        {
                            if (!imageService.ContainsMatch(NodImages.Exit, ScreenSection.Game))
                            {
                                MovePlantToInventory();
                                return;
                            }

                            if (!imageService.ContainsTemplateMatch(NodImages.GardenDust, ScreenSection.MiniGameNoOffset, points[i, j].Rect))
                            {
                                icons[i, j] = null;
                                continue;
                            }

                            Task.Delay(1000).Wait();

                            clickPoint(i,j).Wait();

                            var icon = imageService.CaptureRect(ScreenSection.MiniGameNoOffset, points[i, j].Rect, String.Format("gardening\\slot{0}-{1}.png", i, j));
                            icons[i, j] = icon;


                            if (firstPoint == null) logger.info("first");
                            else logger.info("second");

                            Task.Delay(250).Wait();

                            if (firstPoint == null)
                            {
                                firstPoint = new Point(i, j);
                                if(CheckMatch(i, j))
                                {
                                    firstPoint = null;
                                }
                            }
                            else
                            {
                                Point temp = firstPoint.Value;
                                firstPoint = null;

                                if (imageService.isMatch(icons[i,j], icons[temp.X,temp.Y])) continue;

                                Point? match = HasMatch(i, j);
                                if (match == null) continue;
                                if (match.Value.X == temp.X && match.Value.Y == temp.Y) continue;

                                j--;  
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    logger.error(ex.Message);
                    logger.error(ex.StackTrace);
                }
            }

            MovePlantToInventory();

        }

        private bool CheckMatch(int x, int y)
        {
            Point? match = HasMatch(x, y);
            if (match == null) return false;
            int i = match.Value.X;
            int j = match.Value.Y;

            Task.Delay(1000).Wait();
            clickPoint(i, j).Wait();

            nodInputService.MoveCursorTo(40, 50);
            Task.Delay(150).Wait();

            return true;
        }

        private bool IsSpotActive(int i, int j)
        {
            Task.Delay(500).Wait();

            var images = new List<string>() { NodImages.GardenDust, String.Format("{0}gardening\\slot{1}-{2}.png", NodImages.BASE, i,j) };

            var result1 = imageService.FindTemplateMatch(images, ScreenSection.MiniGameNoOffset, rect: points[i, j].Rect, threshold:0.75);

            bool isInactive = result1[0] != null; // Inactive when dust icon is present
            bool noIcon = result1[1] == null; // Resin image behind dust is not visible

            if (!isInactive && !noIcon) logger.error(String.Format("Slot({0},{1}) is active", i, j));

            // To be active: Dust icon must be gone, and icon must be present
            return !isInactive && !noIcon;
        }

        private Point? HasMatch(int x, int y)
        {
            Task.Delay(250).Wait();

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    if (i == x && j == y) continue;
                    if (icons[i, j] == null) continue;

                    if (imageService.isMatch(icons[x, y], icons[i, j]))
                    {
                        if (!imageService.ContainsTemplateMatch(NodImages.GardenDust, ScreenSection.MiniGameNoOffset, points[i, j].Rect))
                        {
                            logger.info(String.Format("Setting ({0},{1}) to empty", i, j));
                            icons[i, j] = null;
                            continue;
                        }

                        return new Point(i, j);
                    }
                }
            }

            return null;
        }

        private async Task clickPoint(int x, int y)
        {
            nodInputService.inputService.leftClick(points[x, y]);
            nodInputService.MoveCursorTo(50, 40);
            Task.Delay(150).Wait();
        }

        private void MovePlantToInventory()
        {
            UIPoint emptyInventory = inventoryService.GetFirstEmptyInventorySpace();
            if (emptyInventory != null) nodInputService.ClickOnPoint(emptyInventory.X, emptyInventory.Y, true);
            else
            {
                var emptyStorage = inventoryService.GetFirstEmptyStorageSpace();
                if (emptyStorage != null) nodInputService.ClickOnPoint(emptyInventory.X, emptyInventory.Y, true);
                else tokenSource.Cancel();
            }

            logger.info("Done with gradening..");

        }
    }
}