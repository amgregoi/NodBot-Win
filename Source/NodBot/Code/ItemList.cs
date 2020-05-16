using NodBot.Code.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class Item
    {
        public ItemType itemType;
        public int id;
        public String imageFile;

        public Item(int id, ItemType itemType, String file)
        {
            this.id = id;
            this.itemType = itemType;
            this.imageFile = file;
        }
    }


    class ItemList
    {
        private List<Item> itemList;
        public List<Item> itemWhiteList = new List<Item>();
        public List<Item> itemBlackList = new List<Item>();

        private static ItemList instance = null;
        private static readonly object padlock = new object();

        public static ItemList Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null) instance = new ItemList();
                    return instance;
                }
            }
        }


        public void addItem(Item item)
        {
            if (itemBlackList.Contains(item)) return;

            if (!itemWhiteList.Contains(item))
            {
                itemWhiteList.Add(item);
            }
        }

        public ItemList()
        {
            itemWhiteList.Add(new Item(10001, ItemType.Trophy, "Images\\trophy\\NDreadMountain\\t1.png"));
            itemWhiteList.Add(new Item(10002, ItemType.Trophy, "Images\\trophy\\NDreadMountain\\t2.png"));
            itemWhiteList.Add(new Item(10003, ItemType.Trophy, "Images\\trophy\\NDreadMountain\\t3.png"));
            itemWhiteList.Add(new Item(10004, ItemType.Trophy, "Images\\trophy\\NDreadMountain\\t4.png"));

            itemWhiteList.Add(new Item(10011, ItemType.Trophy, "Images\\trophy\\NorthPassage\\t1.png"));
            itemWhiteList.Add(new Item(10012, ItemType.Trophy, "Images\\trophy\\NorthPassage\\t2.png"));
            itemWhiteList.Add(new Item(10013, ItemType.Trophy, "Images\\trophy\\NorthPassage\\t3.png"));
            itemWhiteList.Add(new Item(10014, ItemType.Trophy, "Images\\trophy\\NorthPassage\\t4.png"));

            itemWhiteList.Add(new Item(10021, ItemType.Trophy, "Images\\trophy\\Rocklands\\t1.png"));
            itemWhiteList.Add(new Item(10022, ItemType.Trophy, "Images\\trophy\\Rocklands\\t2.png"));
            itemWhiteList.Add(new Item(10023, ItemType.Trophy, "Images\\trophy\\Rocklands\\t3.png"));
            itemWhiteList.Add(new Item(10024, ItemType.Trophy, "Images\\trophy\\Rocklands\\t4.png"));

            itemWhiteList.Add(new Item(10031, ItemType.Trophy, "Images\\trophy\\SouthPassage\\t1.png"));
            itemWhiteList.Add(new Item(10032, ItemType.Trophy, "Images\\trophy\\SouthPassage\\t2.png"));
            itemWhiteList.Add(new Item(10033, ItemType.Trophy, "Images\\trophy\\SouthPassage\\t3.png"));
            itemWhiteList.Add(new Item(10034, ItemType.Trophy, "Images\\trophy\\SouthPassage\\t4.png"));


            itemWhiteList.Add(new Item(100, ItemType.Ore, NodImages.Ore_T1));


            itemBlackList.Add(new Item(1001, ItemType.Silk, NodImages.Silk_T1));
            itemBlackList.Add(new Item(1004, ItemType.Silk, NodImages.Silk_T4));
            itemBlackList.Add(new Item(1006, ItemType.Silk, NodImages.Silk_T6));
            itemBlackList.Add(new Item(1008, ItemType.Silk, NodImages.Silk_T8));

            // TODO :: read items.json file and populate initial item list  
        }

        private void setupWhiteList()
        {

        }

        private void setupBlackList()
        {

        }

        public Item getItemById(int id)
        {
            var item = itemList.FirstOrDefault(it => it.id == id);
            return item;
        }

        // Trophies
        public static Item Trophy1 = new Item(id: 1, itemType: Enums.ItemType.Trophy, file: NodImages.Trophy1);
        public static Item Trophy2 = new Item(id: 2, itemType: Enums.ItemType.Trophy, file: NodImages.Trophy2);
        public static Item Trophy3 = new Item(id: 3, itemType: Enums.ItemType.Trophy, file: NodImages.Trophy3);
        public static Item Trophy4 = new Item(id: 4, itemType: Enums.ItemType.Trophy, file: NodImages.Trophy4);
    }
}
