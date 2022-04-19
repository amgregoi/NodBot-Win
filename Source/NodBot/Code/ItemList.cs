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
        }

        public void toggleGarden(bool isEnable)
        {
            if (!isEnable)
            {
                itemWhiteList.RemoveAll(item => item.id > 20000 && item.id < 21000);
                itemBlackList.RemoveAll(item => item.id > 20000 && item.id < 21000);
                return;
            }

            itemWhiteList.Add(new Item(20017, ItemType.Resource, "Images\\vegetable\\t17.png"));
            itemWhiteList.Add(new Item(20018, ItemType.Resource, "Images\\vegetable\\t18.png"));
            itemWhiteList.Add(new Item(20019, ItemType.Resource, "Images\\vegetable\\t19.png"));
            itemWhiteList.Add(new Item(20020, ItemType.Resource, "Images\\vegetable\\t20.png"));
            itemWhiteList.Add(new Item(20021, ItemType.Resource, "Images\\vegetable\\t21.png"));
            itemWhiteList.Add(new Item(20022, ItemType.Resource, "Images\\vegetable\\t22.png"));
            itemWhiteList.Add(new Item(20023, ItemType.Resource, "Images\\vegetable\\t23.png"));
            itemWhiteList.Add(new Item(20024, ItemType.Resource, "Images\\vegetable\\t24.png"));
            itemWhiteList.Add(new Item(20025, ItemType.Resource, "Images\\vegetable\\t25.png"));
            itemWhiteList.Add(new Item(20026, ItemType.Resource, "Images\\vegetable\\t26.png"));


            //            itemBlackList.Add(new Item(201, ItemType.Resource, "Images\\vegetable\\t1.png"));
            //            itemBlackList.Add(new Item(202, ItemType.Resource, "Images\\vegetable\\t2.png"));
            itemBlackList.Add(new Item(20003, ItemType.Resource, "Images\\vegetable\\t3.png"));
            itemBlackList.Add(new Item(20004, ItemType.Resource, "Images\\vegetable\\t4.png"));
            itemBlackList.Add(new Item(20005, ItemType.Resource, "Images\\vegetable\\t5.png"));
            itemBlackList.Add(new Item(20006, ItemType.Resource, "Images\\vegetable\\t6.png"));
            itemBlackList.Add(new Item(20007, ItemType.Resource, "Images\\vegetable\\t7.png"));
            itemBlackList.Add(new Item(20008, ItemType.Resource, "Images\\vegetable\\t8.png"));
            itemBlackList.Add(new Item(20009, ItemType.Resource, "Images\\vegetable\\t9.png"));
            itemBlackList.Add(new Item(20010, ItemType.Resource, "Images\\vegetable\\t10.png"));
            itemBlackList.Add(new Item(20011, ItemType.Resource, "Images\\vegetable\\t11.png"));
            itemBlackList.Add(new Item(20012, ItemType.Resource, "Images\\vegetable\\t12.png"));
            itemBlackList.Add(new Item(20013, ItemType.Resource, "Images\\vegetable\\t13.png"));
            itemBlackList.Add(new Item(20014, ItemType.Resource, "Images\\vegetable\\t14.png"));
            itemBlackList.Add(new Item(20015, ItemType.Resource, "Images\\vegetable\\t15.png"));
            itemBlackList.Add(new Item(20016, ItemType.Resource, "Images\\vegetable\\t16.png"));
        }

        public void toggleMining(bool isEnable)
        {
            if (!isEnable)
            {
                itemWhiteList.RemoveAll(item => item.id > 100 && item.id < 150);
                itemBlackList.RemoveAll(item => item.id > 100 && item.id < 150);
                return;
            }

            itemWhiteList.Add(new Item(117, ItemType.Resource, "Images\\ore\\t17.png"));
            itemWhiteList.Add(new Item(118, ItemType.Resource, "Images\\ore\\t18.png"));
            itemWhiteList.Add(new Item(119, ItemType.Resource, "Images\\ore\\t19.png"));
            itemWhiteList.Add(new Item(120, ItemType.Resource, "Images\\ore\\t20.png"));
            itemWhiteList.Add(new Item(121, ItemType.Resource, "Images\\ore\\t21.png"));
            itemWhiteList.Add(new Item(122, ItemType.Resource, "Images\\ore\\t22.png"));
            itemWhiteList.Add(new Item(123, ItemType.Resource, "Images\\ore\\t23.png", 0.95));
            itemWhiteList.Add(new Item(124, ItemType.Resource, "Images\\ore\\t24.png"));
            itemWhiteList.Add(new Item(125, ItemType.Resource, "Images\\ore\\t25.png"));
            itemWhiteList.Add(new Item(126, ItemType.Resource, "Images\\ore\\t26.png"));
            itemWhiteList.Add(new Item(127, ItemType.Resource, "Images\\ore\\t27.png"));
            itemWhiteList.Add(new Item(128, ItemType.Resource, "Images\\ore\\t28.png"));
            itemWhiteList.Add(new Item(129, ItemType.Resource, "Images\\ore\\t29.png"));
            itemWhiteList.Add(new Item(130, ItemType.Resource, "Images\\ore\\t30.png"));


            itemBlackList.Add(new Item(101, ItemType.Resource, "Images\\ore\\t1.png", 0.95));
            itemBlackList.Add(new Item(102, ItemType.Resource, "Images\\ore\\t2.png"));
            itemBlackList.Add(new Item(103, ItemType.Resource, "Images\\ore\\t3.png"));
            itemBlackList.Add(new Item(104, ItemType.Resource, "Images\\ore\\t4.png"));
            itemBlackList.Add(new Item(105, ItemType.Resource, "Images\\ore\\t5.png", 0.95));
            itemBlackList.Add(new Item(106, ItemType.Resource, "Images\\ore\\t6.png"));
            itemBlackList.Add(new Item(107, ItemType.Resource, "Images\\ore\\t7.png"));
            itemBlackList.Add(new Item(108, ItemType.Resource, "Images\\ore\\t8.png"));
            itemBlackList.Add(new Item(109, ItemType.Resource, "Images\\ore\\t9.png"));
            itemBlackList.Add(new Item(110, ItemType.Resource, "Images\\ore\\t10.png"));
            itemBlackList.Add(new Item(111, ItemType.Resource, "Images\\ore\\t11.png"));
            itemBlackList.Add(new Item(112, ItemType.Resource, "Images\\ore\\t12.png"));
            itemBlackList.Add(new Item(113, ItemType.Resource, "Images\\ore\\t13.png", 0.80));
            itemBlackList.Add(new Item(114, ItemType.Resource, "Images\\ore\\t14.png"));
            itemBlackList.Add(new Item(115, ItemType.Resource, "Images\\ore\\t15.png"));
            itemBlackList.Add(new Item(116, ItemType.Resource, "Images\\ore\\t16.png"));
        }

        public Item getItemById(int id)
        {
            var item = itemList.FirstOrDefault(it => it.id == id);
            return item;
        }
    }
}
