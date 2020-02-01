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
        public static String Arena { get { return new NodImages("arena.png").PATH; } }
        public static String ArenaVerify { get { return new NodImages("arena_queue_check.png").PATH; } }
        public static String PlayerResourceMinimum { get { return new NodImages("player_resource_minimum.png").PATH; } }

        public static String Temp_Inventory_1 { get { return new NodImages("temp_inv_1.png").PATH; } }
        public static String Temp_Inventory_2 { get { return new NodImages("temp_inv_2.png").PATH; } }


        public static String Town4 { get { return new NodImages("town4.png").PATH; } }
        public static String Town5 { get { return new NodImages("town5.png").PATH; } }

        public static String Empty { get { return new NodImages("empty.png").PATH; } }

        public static String Empty_Black { get { return new NodImages("empty_inventory.png").PATH; } }
        public static String Empty_Black_Filtered { get { return new NodImages("empty_inventory_filtered.png").PATH; } }

        // Player Specific
        public static String CurrentSS { get { return new NodImages(Settings.Player.playerName+"\\current_ss_.png").PATH; } }
        public static String CurrentSS_Right { get { return new NodImages(Settings.Player.playerName + "\\current_ss_right_.png").PATH; } }
        public static String CurrentSS_Verify_Item { get { return new NodImages(Settings.Player.playerName + "\\current_ss_verify_item_.png").PATH; } }
        public static String NeutralSS { get { return new NodImages(Settings.Player.playerName + "\\player_neutral_.png").PATH; } }

        // MISC
        public static String Test { get { return new NodImages("test_ss.png").PATH; } }
        public static String Test2 { get { return new NodImages("test2_ss.png").PATH; } }


        public static String CompareResult { get { return new NodImages(Settings.Player.playerName + "\\compare_result.png").PATH; } }
        public static String CompareResultX { get { return new NodImages(Settings.Player.playerName + "\\compare_result_x.png").PATH; } }
        public static String CompareResultY { get { return new NodImages(Settings.Player.playerName + "\\compare_result_y.png").PATH; } }

        public static String StatDistribution { get { return new NodImages("stats.png").PATH; } }
        public static String X { get { return new NodImages("x.png").PATH; } }
        public static String Gate { get { return new NodImages("gate.png").PATH; } }

        public static String Trophy1 { get { return new NodImages("trophy\\" + Settings.ZONE + "\\t1.png").PATH; } }
        public static String Trophy2 { get { return new NodImages("trophy\\" + Settings.ZONE + "\\t2.png").PATH; } }
        public static String Trophy3 { get { return new NodImages("trophy\\" + Settings.ZONE + "\\t3.png").PATH; } }
        public static String Trophy4 { get { return new NodImages("trophy\\" + Settings.ZONE + "\\t4.png").PATH; } }

    }
}
