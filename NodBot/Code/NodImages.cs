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
        private static String mParent = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\images\";
        private static String mPath; 
        private NodImages(String aFile) { mPath = mParent + aFile; }

        public String PATH { get { return mPath; } }

        public static String Exit { get { return new NodImages("exit.png").PATH; } }
        public static String Chest1 { get { return new NodImages("chest1.png").PATH; } }
        public static String Chest2 { get { return new NodImages("chest2.png").PATH; } }
        public static String Chest3 { get { return new NodImages("chest3.png").PATH; } }
        public static String Recall { get { return new NodImages("recall.png").PATH; } }
        public static String Handbook { get { return new NodImages("handbook.png").PATH; } }



        public static String CurrentSS { get { return new NodImages("current_ss.png").PATH; } }
        public static String Test { get { return new NodImages("test_ss.png").PATH; } }
    }
}
