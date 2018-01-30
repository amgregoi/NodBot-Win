using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class NodImages
    {
        private static String mPath; 
        private NodImages(String aFile) { mPath = "Images\\" + aFile; }

        public String PATH { get { return mPath; } }

        public static String Exit { get { return new NodImages("exit.png").PATH; } }
        public static String Chest1 { get { return new NodImages("chest1.png").PATH; } }
        public static String Chest2 { get { return new NodImages("chest2.png").PATH; } }
        public static String Chest3 { get { return new NodImages("chest3.png").PATH; } }
        public static String Recall { get { return new NodImages("recall.png").PATH; } }
        public static String Handbook { get { return new NodImages("handbook.png").PATH; } }
        public static String Dust { get { return new NodImages("dust.png").PATH; } }
        public static String TownWalk { get { return new NodImages("tw_message.png").PATH; } }


        public static String InCombat { get { return new NodImages("incombat.png").PATH; } }


        public static String Town4 { get { return new NodImages("town4.png").PATH; } }
        public static String Town5 { get { return new NodImages("town5.png").PATH; } }


        public static String Trophy1 { get { return new NodImages("t1.png").PATH; } }
        public static String Trophy2 { get { return new NodImages("t2.png").PATH; } }
        public static String Trophy3 { get { return new NodImages("t3.png").PATH; } }
        public static String Trophy4 { get { return new NodImages("t4.png").PATH; } }
        public static String Empty { get { return new NodImages("empty.png").PATH; } }


        public static String CurrentSS { get { return new NodImages("current_ss.png").PATH; } }
        public static String Test { get { return new NodImages("test_ss.png").PATH; } }
    }
}
