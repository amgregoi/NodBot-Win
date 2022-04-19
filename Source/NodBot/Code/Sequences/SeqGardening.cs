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

        private static int xOff = 15, yOff = 38, width = 60, height = 54;

        UIPoint[,] points = new UIPoint[4, 4]
            {
                { UIPoint.New(new Rectangle(250+xOff,153+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,153+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,153+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,153+yOff,width, height)) },
                { UIPoint.New(new Rectangle(250+xOff,233+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,233+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,233+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,233+yOff,width, height)) },
                { UIPoint.New(new Rectangle(250+xOff,313+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,313+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,313+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,313+yOff,width, height)) },
                { UIPoint.New(new Rectangle(250+xOff,393+yOff,width, height)),UIPoint.New(new Rectangle(330+xOff,393+yOff,width, height)),UIPoint.New(new Rectangle(410+xOff,393+yOff,width, height)),UIPoint.New(new Rectangle(490+xOff,393+yOff,width, height)) },
            };

        Image<Bgr, byte>[,] icons;

        Boolean[,] isComplete;

        private void init()
        {
            isComplete = new Boolean[4, 4]
            {
                {false, false, false, false },
                {false, false, false, false },
                {false, false, false, false },
                {false, false, false, false }
            };

            icons = new Image<Bgr, byte>[4, 4]
            {
                {null, null,null,null },
                {null, null,null,null },
                {null, null,null,null },
                {null, null,null,null }
            };
        }

        public override async Task Start()
        {
            combatState = SequenceState.RESOURCING;
            var iconPoint = imageService.FindTemplateMatch(NodImages.GardeningIcon, screenSection: ScreenSection.Game);

            if (iconPoint != null)
            {
                nodInputService.ClickOnPoint(iconPoint.X, iconPoint.Y, true, true);
                startGarden();
            }
            else
            {
                startGarden();
            }
        }

        private bool isRepeat = false;
        private async Task startGarden()
        {
            init();
            Task.Delay(500).Wait();
            while (imageService.ContainsMatch(NodImages.Exit, ScreenSection.Game))
            {
                try
                {
                    Point? firstPoint = null;
                    for (int i = 0; i < points.GetLength(0); i++)
                    {
                        if (!imageService.ContainsMatch(NodImages.Exit, ScreenSection.Game))
                        {
                            MovePlantToInventory();
                            return;
                        }

                        for (int j = 0; j < points.GetLength(1); j++)
                        {


                            //if (isComplete[i, j]) continue;


                            if (!isRepeat && !imageService.ContainsTemplateMatch(NodImages.GardenDust, ScreenSection.MiniGameNoOffset, points[i, j].Rect))
                            {
                                logger.info(String.Format("Setting ({0},{1}) to empty", i, j));
                                icons[i, j] = null;
                                isComplete[i, j] = true;
                                continue;
                            }
                            else isRepeat = false;

                            Task.Delay(1250).Wait();

                            clickPoint(i, j).Wait();

                            Task.Delay(150).Wait();

                            var iconRect = points[i, j].Rect;
                            iconRect.Width = 20;
                            iconRect.Height = 20;
                            iconRect.X += 20;
                            iconRect.Y += 25;

                            var icon = imageService.CaptureRect(ScreenSection.MiniGameNoOffset, points[i, j].Rect, String.Format("gardening\\slot{0}-{1}.png", i, j));
                            icons[i, j] = icon;

                            Task.Delay(250).Wait();

                            if (firstPoint == null)
                            {
                                logger.info(String.Format("first ({0},{1})", i,j));

                                firstPoint = new Point(i, j);
                                if (CheckMatch(i, j))
                                {
                                    firstPoint = null;
                                }
                            }
                            else
                            {
                                logger.info(String.Format("second ({0},{1})", i, j));

                                Point temp = firstPoint.Value;
                                firstPoint = null;

                                if (imageService.isMatch(icons[i, j], icons[temp.X, temp.Y], 0.70f))
                                {
                                    logger.info("First and Second were a match");
                                    icons[i, j] = null;
                                    icons[temp.X, temp.Y] = null;
                                    continue;
                                }

                                Point? match = HasMatch(i, j, temp);
                                if (match == null) continue;
                                if (match.Value.X == temp.X && match.Value.Y == temp.Y) 
                                    continue;

                                logger.info(String.Format("Found a previous match ({0},{1}) with Second",match.Value.X, match.Value.Y));
                                //Task.Delay(750).Wait(); // Wait for veggie icons to fade to get an accurate screen reading
                                j--;
                                isRepeat = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.error(ex.Message);
                    logger.error(ex.StackTrace);
                }
            }

            MovePlantToInventory();
            Task.Delay(250).Wait();
            MoveSilkToInventory();
            logger.info("Done with gradening..");
        }

        private bool CheckMatch(int x, int y)
        {
            Point? match = HasMatch(x, y);
            if (match == null) return false;

            int i = match.Value.X;
            int j = match.Value.Y;


            logger.info("Had previous match");
            logger.info(String.Format("second ({0},{1})", i, j));

            Task.Delay(1250).Wait();
            clickPoint(i, j).Wait();

            nodInputService.MoveCursorTo(40, 50);
            Task.Delay(150).Wait();

            return true;
        }

        private bool IsSpotActive(int i, int j)
        {
            Task.Delay(500).Wait();

            var images = new List<string>() { NodImages.GardenDust, String.Format("{0}gardening\\slot{1}-{2}.png", NodImages.BASE, i, j) };

            var result1 = imageService.FindTemplateMatch(images, ScreenSection.MiniGameNoOffset, rect: points[i, j].Rect, threshold: 0.75);

            bool isInactive = result1[0] != null; // Inactive when dust icon is present
            bool noIcon = result1[1] == null; // Resin image behind dust is not visible

            if (!isInactive && !noIcon) logger.error(String.Format("Slot({0},{1}) is active", i, j));

            // To be active: Dust icon must be gone, and icon must be present
            return !isInactive && !noIcon;
        }

        private Point? HasMatch(int x, int y, Point? skip = null)
        {
            Task.Delay(250).Wait();

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    if (i == x && j == y) continue;
                    if (skip != null && i == skip.Value.X && j == skip.Value.Y) continue;
                    if (icons[i, j] == null) continue;


                    if (imageService.isMatch(icons[x, y], icons[i, j], 0.70f))
                    {
                        if (!imageService.ContainsTemplateMatch(NodImages.GardenDust, ScreenSection.MiniGameNoOffset, points[i, j].Rect))
                        {
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
            nodInputService.inputService.doubleLeftClick(points[x, y]);
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
        }

        private void MoveSilkToInventory()
        {
            UIPoint emptyInventory = inventoryService.GetFirstEmptyInventorySpace();
            if (emptyInventory != null) nodInputService.ClickOnPoint(emptyInventory.X, emptyInventory.Y, true);
            else
            {
                var emptyStorage = inventoryService.GetFirstEmptyStorageSpace();
                if (emptyStorage != null) nodInputService.ClickOnPoint(emptyInventory.X, emptyInventory.Y, true);
                else tokenSource.Cancel();
            }
        }
    }
}