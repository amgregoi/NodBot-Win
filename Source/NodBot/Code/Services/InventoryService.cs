using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    public class InventoryService
    {
        // Right of game window (inventory)
        private static int xPosition = 950;

        // Bottom of game window (storage)
        private static int yPosition = 800;

        private List<Rectangle> inventory;
        private List<Rectangle> storage;
        private ImageService imageAnalysis = new ImageService();
        private InputService mouseInput;
        public InventoryService(InputService input)
        {
            mouseInput = input;
            //inventory = imageAnalysis.FindTemplateMatchWithXConstraint(NodImages.CurrentSS, NodImages.Empty_Black, xPosition, false);
            storage = imageAnalysis.FindTemplateMatchWithYConstraint(NodImages.Empty_Black, yPosition, false)
                .OrderBy(item => item.Y)
                .ThenBy(item => item.X).ToList();
        }

        public bool isStorageEmpty()
        {
            return storage.Count == 0;
        }

        public List<Rectangle> getItemLocations(String itemImage)
        {
            var items = imageAnalysis.FindTemplateMatchWithXConstraint(itemImage, xPosition, false)
                .OrderByDescending(item => item.Y)
                .ThenByDescending(item => item.X)
                .ToList();
            return items;
        }

        public void sortInventory()
        {
            if (storage != null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        // TODO :: Build system to manage zone and relavent trophies
                        stackItems(NodImages.SDread_Trophy1).Wait();
                        stackItems(NodImages.SDread_Trophy2).Wait();
                        stackItems(NodImages.SDread_Trophy3).Wait();
                        stackItems(NodImages.SDread_Trophy4).Wait();
                    }
                    catch (AggregateException ex)
                    {
                        Console.Out.WriteLine(ex.StackTrace);
                        Console.Out.WriteLine(ex.InnerException.StackTrace);
                    }
                });
            }
            else Console.Out.WriteLine("Storage was not setup properly");
        }


        public async Task stackItems(String itemImage)
        {
            if (storage.Count == 0)
            {
                Console.Out.WriteLine("Storage full");
                storage = imageAnalysis.FindTemplateMatchWithYConstraint(NodImages.Empty_Black, yPosition, true);
                if (storage.Count == 0) return;
            }

            List<Rectangle> items = getItemLocations(itemImage);

            Console.Out.WriteLine("Starting item stack for: " + itemImage);
            while (items.Count > 1)
            {

                Rectangle rect0 = items[0];
                Rectangle rect1 = items[1];
                int x0 = rect0.X + (rect0.Width/ 2);
                int y0 = rect0.Y + (rect0.Height/ 2);
                int x1 = rect1.X + (rect1.Width/ 2);
                int y1 = rect1.Y + (rect1.Height/ 2);

                //Task.Delay(350).Wait();
                //mouseInput.sendShiftLeftDown(x0, y0);
                //Task.Delay(750).Wait();
                //mouseInput.sendShiftLeftDown(x1, y1,false);
                mouseInput.dragTo(x0, y0, x1, y1, true).Wait();

                // Move cursor out of the way
                Task.Delay(250).Wait();
                mouseInput.moveMouse(20, 50);
                Task.Delay(400).Wait();

                bool isSlotEmpty = imageAnalysis.isRectEmpty(rect0, NodImages.CurrentSS_Verify_Item);
                if(isSlotEmpty)
                {

                    Console.Out.WriteLine("Empty slot ~~ WOOOOOOOO");
                    items.RemoveAt(0);
                }
                else
                {
                    Console.Out.WriteLine("Well the slot was not empty");
                    // Move to storage
                    Rectangle storageSlot = storage[0];
                    //Rectangle storageSlot = getEmptyStorageSlot().GetValueOrDefault(); 
                    // TODO :: Find and cache storage on bot start
                    // when storage is filled we will turn bot off
                    // Can make this a settings in the config file

                    if(storageSlot == null)
                    {
                        // Stop bot ?
                        return;
                    }

                    int xStorage = storageSlot.X + (storageSlot.Width/2);
                    int yStorage = storageSlot.Y + (storageSlot.Height/2);

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

        private int randomWithMax(int max)
        {
            return new Random().Next(max);
        }
    }
}
