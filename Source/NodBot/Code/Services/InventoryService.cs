using NodBot.Code.Enums;
using NodBot.Code.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodBot.Code.Services
{
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

        public bool isRunning = false;

        private List<UIPoint> storage;
        private readonly ImageService imageService = new ImageService(InputService.getNodiatisWindowHandle());
        private readonly InputService mouseInput;

        public bool IsStorageEmpty => storage.Count == 0;

        public InventoryService(InputService input)
        {
            instance = this;
            mouseInput = input;

            Task.Run(() =>
            {
                storage = imageService.FindTemplateMatches(NodImages.Empty_Black, ScreenSection.Storage)
                    .OrderBy(item => item.Y)
                    .ThenBy(item => item.X).ToList();
            });
        }

        public List<UIPoint> getItemLocations(String itemImage)
        {
            List<UIPoint> items = imageService.FindTemplateMatches(itemImage, ScreenSection.Inventory, threshold:0.85)
                .OrderByDescending(item => item.Y)
                .ThenByDescending(item => item.X)
                .ToList();
            return items;
        }

        public async Task SortInventory()
        {
            try
            {
                isRunning = true;
                ItemList items = ItemList.Instance;

                imageService.ScanForItems(ScreenSection.Inventory, items.itemWhiteList, items.itemBlackList, out List<Item> whiteList, out List<Item> blackList);

                foreach (Item item in whiteList)
                {
                    StackItems(item.imageFile).Wait();
                }

                foreach (Item item in blackList)
                {
                    StackItems(item.imageFile).Wait();

                    Console.Out.WriteLine("Deleting item.. " + item.imageFile);
                    var itemPoint = imageService.FindTemplateMatch(item.imageFile, screenSection: ScreenSection.Inventory, threshold: item.threshold);
                    if(itemPoint != null) DeleteItem(itemPoint).Wait();
                    else Console.Out.WriteLine("Failed to find item..");
                }
            }
            catch (AggregateException ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
                Console.Out.WriteLine(ex.InnerException.StackTrace);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
            }
            finally
            {
                isRunning = false;
            }
        }


        public async Task StackItems(String itemImage)
        {
            List<UIPoint> items = getItemLocations(itemImage);

            Console.Out.WriteLine("Starting item stack for: " + itemImage);
            while (items.Count > 1)
            {

                UIPoint item0 = items[0];
                UIPoint item1 = items[1];

                mouseInput.dragTo(item0.X, item0.Y, item1.X, item1.Y, true).Wait();

                // Move cursor out of the way
                Task.Delay(250).Wait();
                mouseInput.moveMouse(100, 100);
                //Task.Delay(100).Wait();
                // mouseInput.leftClick();
                Task.Delay(400).Wait();

                bool isSlotEmpty = imageService.IsRectEmpty(item0.Rect, NodImages.GameWindow, screenSection: ScreenSection.Inventory);

                if (isSlotEmpty)
                {
                    items.RemoveAt(0);
                }
                else
                {
                    if (storage == null || storage.Count == 0)
                    {
                        items.RemoveAt(0);

                        storage = imageService.FindTemplateMatches(NodImages.Empty_Black, ScreenSection.Storage);
                        if (storage.Count == 0)
                        {
                            Console.Out.WriteLine("Storage full");
                            return;
                        }
                    }

                    // Move to storage
                    UIPoint storageSlot = storage[0];
                    //Rectangle storageSlot = getEmptyStorageSlot().GetValueOrDefault(); 
                    // TODO :: Find and cache storage on bot start
                    // when storage is filled we will turn bot off
                    // Can make this a settings in the config file

                    if (storageSlot == null)
                    {
                        // Stop bot ?
                        return;
                    }

                    mouseInput.dragTo(item1.X, item1.Y, storageSlot.X, storageSlot.Y, true).Wait();

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

        public UIPoint GetFirstEmptyInventorySpace()
        {
            return imageService.FindTemplateMatch(NodImages.Empty_Black, ScreenSection.Inventory);
        }

        public UIPoint GetFirstEmptyStorageSpace()
        {
            return imageService.FindTemplateMatch(NodImages.Empty_Black, screenSection: ScreenSection.Storage);
        }

        private int menuItemHeight = 16;
        public async Task DeleteItem(UIPoint item)
        {
            mouseInput.rightClick(item);

            Task.Delay(100).Wait();

            var destroy = imageService.FindTemplateMatch(NodImages.DestroyItem, ScreenSection.Inventory, threshold:0.55);
            if (destroy == null) return;
            mouseInput.leftClick(destroy);

            //var destroyItem = new UIPoint(item.X + RandomInRange(10, 50), item.Y + (10 * menuItemHeight) + (menuItemHeight/2));
            ////mouseInput.leftClick(destroyItem);
            //mouseInput.leftClick(destroyItem);

            Task.Delay(100).Wait();
            //mouseInput.moveMouse(305, 405); // 350, 420 -> center

            // Note:: scanning for template fails because of floating combat text
            mouseInput.leftClick(new UIPoint(350 + RandomInRange(-30, 30), 420 + RandomInRange(-10, 10)));
            Task.Delay(75).Wait();
            mouseInput.leftClick(new UIPoint(350 + RandomInRange(-30, 30), 420 + RandomInRange(-10, 10)));

            Task.Delay(500).Wait();
        }

        private int RandomWithMax(int max)
        {
            return new Random().Next(max);
        }

        private int RandomInRange(int min, int max)
        {
            return new Random().Next(min, max);
        }
    }
}
