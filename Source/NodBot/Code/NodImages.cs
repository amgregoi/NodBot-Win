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

        public static String BASE { get { return new NodImages("").PATH; } }

        public static String Exit { get { return new NodImages("exit.png").PATH; } }
        public static String ButtonYes { get { return new NodImages("button_yes.png").PATH; } }
        public static String ButtonNo { get { return new NodImages("button_no.png").PATH; } }
        public static String DestroyItem { get { return new NodImages("destroy_item.png").PATH; } }

        public static String Chest1 { get { return new NodImages("chest1.png").PATH; } }
        public static String Chest2 { get { return new NodImages("chest2.png").PATH; } }
        public static String Chest3 { get { return new NodImages("chest3.png").PATH; } }
        public static String Recall { get { return new NodImages("recall.png").PATH; } }
        public static String Handbook { get { return new NodImages("handbook.png").PATH; } }
        public static String Dust { get { return new NodImages("dust.png").PATH; } }
        public static String TownWalk { get { return new NodImages("tw_message.png").PATH; } }

        //Mining
        public static String Rock1 { get { return new NodImages("mining/rock_1.png").PATH; } }
        public static String Rock2{ get { return new NodImages("mining/rock_2.png").PATH; } }
        public static String Rock3 { get { return new NodImages("mining/rock_3.png").PATH; } }
        public static String Rock4 { get { return new NodImages("mining/rock_4.png").PATH; } }
        public static String MiningIcon { get { return new NodImages("mining/icon.png").PATH; } }
        public static String GardeningIcon { get { return new NodImages("gardening/icon.png").PATH; } }
        public static String GardenDust { get { return new NodImages("gardening/garden_base.png").PATH; } }

        // Combat + Arena
        public static String InCombat { get { return new NodImages("incombat.png").PATH; } }
        public static String Arena { get { return new NodImages("arena.png").PATH; } }
        public static String ArenaVerify { get { return new NodImages("arena_queue_check.png").PATH; } }
        public static String PlayerResourceHealth { get { return new NodImages("player_resource_health.png").PATH; } }
        public static String PlayerResourceEnergy { get { return new NodImages("player_resource_energy.png").PATH; } }
        public static String PlayerResourceMana { get { return new NodImages("player_resource_mana.png").PATH; } }

        // Town Walking
        public static String Town4 { get { return new NodImages("town4.png").PATH; } }
        public static String Town5 { get { return new NodImages("town5.png").PATH; } }


        // Inventory
        public static String Empty_Black { get { return new NodImages("empty_inventory.png").PATH; } }
        public static String Empty_Black_Filtered { get { return new NodImages("empty_inventory_filtered.png").PATH; } }
        public static String Empty { get { return new NodImages("empty.png").PATH; } }

        // Player Specific
        public static String NeutralSS { get { return new NodImages(Settings.Player.playerName + "\\player_neutral_.png").PATH; } }
        public static String GameWindow { get { return new NodImages(Settings.Player.playerName + "\\screen.png").PATH; } }
        public static String GameWindow_Game { get { return new NodImages(Settings.Player.playerName + "\\screen_Game.png").PATH; } }
        public static String GameWindow_Inventory { get { return new NodImages(Settings.Player.playerName + "\\screen_Inventory.png").PATH; } }
        public static String GameWindow_Storage { get { return new NodImages(Settings.Player.playerName + "\\screen_Storage.png").PATH; } }

        // Debug
        public static String Temp_Inventory_1 { get { return new NodImages("temp_inv_1.png").PATH; } }
        public static String Temp_Inventory_2 { get { return new NodImages("temp_inv_2.png").PATH; } }
        public static String PlayerDebug { get { return new NodImages(Settings.Player.playerName + "\\__god_damn_updates.png").PATH; } }

        // Misc.
        public static String StatDistribution { get { return new NodImages("stats.png").PATH; } }
        public static String X { get { return new NodImages("x.png").PATH; } }
        public static String X2 { get { return new NodImages("x2.png").PATH; } }
        public static String Gate { get { return new NodImages("gate.png").PATH; } }

        // Ore
        public static String Ore_T1 { get { return new NodImages("ore\\t1.png").PATH; } }
        public static String Ore_T2 { get { return new NodImages("ore\\t2.png").PATH; } }
        public static String Ore_T17 { get { return new NodImages("ore\\t17.png").PATH; } }
        public static String Ore_T18 { get { return new NodImages("ore\\t18.png").PATH; } }
        public static String Ore_T19 { get { return new NodImages("ore\\t19.png").PATH; } }
        public static String Ore_T20 { get { return new NodImages("ore\\t20.png").PATH; } }
        public static String Ore_T21 { get { return new NodImages("ore\\t21.png").PATH; } }
        public static String Ore_T22 { get { return new NodImages("ore\\t22.png").PATH; } }
        public static String Ore_T23 { get { return new NodImages("ore\\t23.png").PATH; } }
        public static String Ore_T24 { get { return new NodImages("ore\\t24.png").PATH; } }
        public static String Ore_T25 { get { return new NodImages("ore\\t25.png").PATH; } }
        public static String Ore_T26 { get { return new NodImages("ore\\t26.png").PATH; } }
        public static String Ore_T27 { get { return new NodImages("ore\\t27.png").PATH; } }
        public static String Ore_T28 { get { return new NodImages("ore\\t28.png").PATH; } }
        public static String Ore_T29 { get { return new NodImages("ore\\t29.png").PATH; } }
        public static String Ore_T30 { get { return new NodImages("ore\\t30.png").PATH; } }


        // Ore
        public static String Silk_T1 { get { return new NodImages("silk\\t1.png").PATH; } }
        public static String Silk_T4 { get { return new NodImages("silk\\t4.png").PATH; } }
        public static String Silk_T6 { get { return new NodImages("silk\\t6.png").PATH; } }
        public static String Silk_T8{ get { return new NodImages("silk\\t8.png").PATH; } }

    }
}
