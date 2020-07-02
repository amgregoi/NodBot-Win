using NodBot.Code.Enums;
using System;
using System.Collections.Generic;
using System.IO;
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
        public double threshold;
        public Item(int id, ItemType itemType, String file, double threshold = 0.85)
        {
            this.id = id;
            this.itemType = itemType;
            this.imageFile = file;
            this.threshold = threshold;
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

            setupWhiteList();
            setupBlackList();

            // TODO :: read items.json file and populate initial item list  
        }

        private void setupWhiteList()
        {
            var trophyId = 10001;
            string[] folderEntries = Directory.GetDirectories("Images\\trophy");
            foreach (String zone in folderEntries)
            {
                var title = zone.Split(new char[] { '\\' }).Last();
                string[] fileEntries = Directory.GetFiles(zone);
                foreach(String file in fileEntries)
                {
                    itemWhiteList.Add(new Item(trophyId, ItemType.Trophy, file));
                }

            }

            itemWhiteList.Add(new Item(1, ItemType.Potion, "Images\\gate.png"));
            itemWhiteList.Add(new Item(2, ItemType.Potion, "Images\\recall.png"));
            itemWhiteList.Add(new Item(3, ItemType.Item, "Images\\heroic_ess.png"));


            itemWhiteList.Add(new Item(117, ItemType.Ore, "Images\\ore\\t17.png"));
            itemWhiteList.Add(new Item(118, ItemType.Ore, "Images\\ore\\t18.png"));
            itemWhiteList.Add(new Item(119, ItemType.Ore, "Images\\ore\\t19.png"));
            itemWhiteList.Add(new Item(120, ItemType.Ore, "Images\\ore\\t20.png"));
            itemWhiteList.Add(new Item(121, ItemType.Ore, "Images\\ore\\t21.png"));
            itemWhiteList.Add(new Item(122, ItemType.Ore, "Images\\ore\\t22.png"));
            itemWhiteList.Add(new Item(123, ItemType.Ore, "Images\\ore\\t23.png", 0.95));
            itemWhiteList.Add(new Item(124, ItemType.Ore, "Images\\ore\\t24.png"));
            itemWhiteList.Add(new Item(125, ItemType.Ore, "Images\\ore\\t25.png"));
            itemWhiteList.Add(new Item(126, ItemType.Ore, "Images\\ore\\t26.png"));
            itemWhiteList.Add(new Item(127, ItemType.Ore, "Images\\ore\\t27.png"));
            itemWhiteList.Add(new Item(128, ItemType.Ore, "Images\\ore\\t28.png"));
            itemWhiteList.Add(new Item(129, ItemType.Ore, "Images\\ore\\t29.png"));
            itemWhiteList.Add(new Item(130, ItemType.Ore, "Images\\ore\\t30.png"));

            itemWhiteList.Add(new Item(1020, ItemType.Silk, "Images\\silk\\t20.png"));
            itemWhiteList.Add(new Item(1021, ItemType.Silk, "Images\\silk\\t21.png"));
            itemWhiteList.Add(new Item(1022, ItemType.Silk, "Images\\silk\\t22.png", 0.95));
            itemWhiteList.Add(new Item(1025, ItemType.Silk, "Images\\silk\\t25.png"));
        }

        /// <summary>
        /// 
        /// </summary>
        private void setupBlackList()
        {
            itemWhiteList.Add(new Item(100, ItemType.Ore, "Images\\ore\\t0.png"));

            itemBlackList.Add(new Item(1001, ItemType.Silk, "Images\\silk\\t1.png"));
            itemBlackList.Add(new Item(1002, ItemType.Silk, "Images\\silk\\t2.png"));
            itemBlackList.Add(new Item(1003, ItemType.Silk, "Images\\silk\\t3.png"));
            itemBlackList.Add(new Item(1004, ItemType.Silk, "Images\\silk\\t4.png"));
            itemBlackList.Add(new Item(1005, ItemType.Silk, "Images\\silk\\t5.png"));
            itemBlackList.Add(new Item(1006, ItemType.Silk, "Images\\silk\\t6.png"));
            itemBlackList.Add(new Item(1007, ItemType.Silk, "Images\\silk\\t7.png"));
            itemBlackList.Add(new Item(1008, ItemType.Silk, "Images\\silk\\t8.png"));
            itemBlackList.Add(new Item(1009, ItemType.Silk, "Images\\silk\\t9.png"));
            itemBlackList.Add(new Item(1010, ItemType.Silk, "Images\\silk\\t10.png"));
            itemBlackList.Add(new Item(1011, ItemType.Silk, "Images\\silk\\t11.png"));
            itemBlackList.Add(new Item(1012, ItemType.Silk, "Images\\silk\\t12.png"));
            itemBlackList.Add(new Item(1013, ItemType.Silk, "Images\\silk\\t13.png"));
            itemBlackList.Add(new Item(1014, ItemType.Silk, "Images\\silk\\t14.png"));
            itemBlackList.Add(new Item(1015, ItemType.Silk, "Images\\silk\\t15.png"));
            itemBlackList.Add(new Item(1016, ItemType.Silk, "Images\\silk\\t16.png"));
            itemBlackList.Add(new Item(1017, ItemType.Silk, "Images\\silk\\t17.png"));
            itemBlackList.Add(new Item(1018, ItemType.Silk, "Images\\silk\\t18.png"));
            itemBlackList.Add(new Item(1019, ItemType.Silk, "Images\\silk\\t19.png"));


            itemBlackList.Add(new Item(101, ItemType.Ore, "Images\\ore\\t1.png", 0.95));
            itemBlackList.Add(new Item(102, ItemType.Ore, "Images\\ore\\t2.png"));
            itemBlackList.Add(new Item(103, ItemType.Ore, "Images\\ore\\t3.png"));
            itemBlackList.Add(new Item(104, ItemType.Ore, "Images\\ore\\t4.png"));
            itemBlackList.Add(new Item(105, ItemType.Ore, "Images\\ore\\t5.png"));
            itemBlackList.Add(new Item(106, ItemType.Ore, "Images\\ore\\t6.png"));
            itemBlackList.Add(new Item(107, ItemType.Ore, "Images\\ore\\t7.png"));
            itemBlackList.Add(new Item(108, ItemType.Ore, "Images\\ore\\t8.png"));
            itemBlackList.Add(new Item(109, ItemType.Ore, "Images\\ore\\t9.png"));
            itemBlackList.Add(new Item(110, ItemType.Ore, "Images\\ore\\t10.png"));
            itemBlackList.Add(new Item(111, ItemType.Ore, "Images\\ore\\t11.png"));
            itemBlackList.Add(new Item(112, ItemType.Ore, "Images\\ore\\t12.png"));
            itemBlackList.Add(new Item(113, ItemType.Ore, "Images\\ore\\t13.png", 0.80));
            itemBlackList.Add(new Item(114, ItemType.Ore, "Images\\ore\\t14.png"));
            itemBlackList.Add(new Item(115, ItemType.Ore, "Images\\ore\\t15.png"));
            itemBlackList.Add(new Item(116, ItemType.Ore, "Images\\ore\\t16.png"));

        }

        public Item getItemById(int id)
        {
            var item = itemList.FirstOrDefault(it => it.id == id);
            return item;
        }
    }
}
