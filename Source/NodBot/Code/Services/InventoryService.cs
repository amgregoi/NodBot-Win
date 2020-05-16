using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    public class GameDimension
    {
        public static int WIDTH = 950;
        public static int HEIGHT = 800;
    }

    public class InventoryService
    {






        private static InventoryService instance = null;
        private static readonly object padlock = new object();

        public static InventoryService Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance;
                }
            }
        }









        // Right of game window (inventory)
        private static int xPosition = 950;

        // Bottom of game window (storage)
        private static int yPosition = 800;

        private List<Rectangle> storage;
        // TODO :: need to cleanup / separate items in ImageService
        private ImageService imageService = new ImageService(InputService.getNodiatisWindowHandle());
        private InputService mouseInput;
        public InventoryService(InputService input)
        {
            instance = this;
            mouseInput = input;
            //inventory = imageAnalysis.FindTemplateMatchWithXConstraint(NodImages.CurrentSS, NodImages.Empty_Black, xPosition, false);

            Task.Run(() =>
            {
                storage = imageService.FindTemplateMatchWithYConstraint(NodImages.Empty_Black, yPosition, false)
                    .OrderBy(item => item.Y)
                    .ThenBy(item => item.X).ToList();
            });
        }

        public bool isStorageEmpty()
        {
            return storage.Count == 0;
        }

        public List<Rectangle> getItemLocations(String itemImage)
        {
            var items = imageService.FindTemplateMatchWithXConstraint(itemImage, xPosition, false)
                .OrderByDescending(item => item.Y)
                .ThenByDescending(item => item.X)
                .ToList();
            return items;
        }

        public async Task sortInventory()
        {
            try
            {
                ItemList items = ItemList.Instance;

                List<Item> whiteList;
                List<Item> blackList;


                imageService.ScanForItems(items.itemWhiteList, items.itemBlackList, out whiteList, out blackList);


                foreach (Item item in whiteList)
                {
                    stackItems(item.imageFile).Wait();
                }

                foreach (Item item in blackList)
                {
                    Console.Out.WriteLine("should be deleting: " + item.imageFile);
                }

                // TODO :: Build system to manage zone and relavent trophies
                //stackItems(NodImages.Trophy1).Wait();
                //stackItems(NodImages.Trophy2).Wait();
                //stackItems(NodImages.Trophy3).Wait();
                //stackItems(NodImages.Trophy4).Wait();

                // TODO :: Build system to blacklist items, and sort threw relevant items that could appear in inventory
                // I.e. if mining - check for ores, if not mining we can skip that group of items etc..
                //stackItems(NodImages.Ore_T1).Wait();
            }
            catch (AggregateException ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
                Console.Out.WriteLine(ex.InnerException.StackTrace);
            }
        }


        public async Task stackItems(String itemImage)
        {
            if (storage == null || storage.Count == 0)
            {
                Console.Out.WriteLine("Storage full");
                storage = imageService.FindTemplateMatchWithYConstraint(NodImages.Empty_Black, yPosition, true);
                if (storage.Count == 0) return;
            }

            List<Rectangle> items = getItemLocations(itemImage);

            Console.Out.WriteLine("Starting item stack for: " + itemImage);
            while (items.Count > 1)
            {

                Rectangle rect0 = items[0];
                Rectangle rect1 = items[1];
                int x0 = rect0.X + (rect0.Width / 2);
                int y0 = rect0.Y + (rect0.Height / 2);
                int x1 = rect1.X + (rect1.Width / 2);
                int y1 = rect1.Y + (rect1.Height / 2);

                mouseInput.dragTo(x0, y0, x1, y1, true).Wait();

                // Move cursor out of the way
                Task.Delay(250).Wait();
                mouseInput.moveMouse(100, 100);
                //Task.Delay(100).Wait();
                // mouseInput.leftClick();
                Task.Delay(400).Wait();

                bool isSlotEmpty = imageService.isRectEmpty(rect0, NodImages.CurrentSS_Verify_Item);
                if (isSlotEmpty)
                {
                    items.RemoveAt(0);
                }
                else
                {
                    if (storage == null)
                    {
                        items.RemoveAt(0);
                        return;
                    }

                    // Move to storage
                    Rectangle storageSlot = storage[0];
                    //Rectangle storageSlot = getEmptyStorageSlot().GetValueOrDefault(); 
                    // TODO :: Find and cache storage on bot start
                    // when storage is filled we will turn bot off
                    // Can make this a settings in the config file

                    if (storageSlot == null)
                    {
                        // Stop bot ?
                        return;
                    }

                    int xStorage = storageSlot.X + (storageSlot.Width / 2);
                    int yStorage = storageSlot.Y + (storageSlot.Height / 2);

                    mouseInput.dragTo(x1, y1, xStorage, yStorage, true).Wait();

                    storage.RemoveAt(0);
                    items.RemoveAt(0);

                    Task.Delay(250).Wait();
                }

                // move item[1] into item[2]
                // scan location of item[1]
                // if(location == empty) items.remove(1)
            }


            Console.Out.WriteLine("Completed item stack for: " + itemImage);
            return;
        }

        public Point? getFirstEmptyInventorySpace()
        {
            Rectangle? match = imageService.FindTemplateMatchWithXConstraintSingle(NodImages.Empty_Black, xPosition, lessThanX: false);
            if (match == null) return null;
            int x0 = match.Value.X + (match.Value.Width / 2);
            int y0 = match.Value.Y + (match.Value.Height / 2);
            return new Point(x0, y0);
        }

        public Point? getFirstEmptyStorageSpace()
        {
            Rectangle? match = imageService.FindTemplateMatchWithYConstraintSingle(NodImages.Empty_Black, yPosition, lessThanY: false);
            if (match == null) return null;
            int x0 = match.Value.X + (match.Value.Width / 2);
            int y0 = match.Value.Y + (match.Value.Height / 2);
            return new Point(x0, y0);
        }

        private int randomWithMax(int max)
        {
            return new Random().Next(max);
        }
    }
}
