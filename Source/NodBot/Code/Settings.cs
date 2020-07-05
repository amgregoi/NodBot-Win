using NodBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class Settings
    {
        public static bool DEBUG { get; set; } = false;
        public static bool CHESTS { get; set; } = true;
        public static bool PILGRIMAGE { get; set; } = false;
        public static bool BOSSING { get; set; } = false;
        public static bool WAIT_FOR_HEALTH { get; set; } = false;
        public static bool WAIT_FOR_ENERGY { get; set; } = false;
        public static bool WAIT_FOR_MANA { get; set; } = false;
        public static bool RESOURCE_MINING { get; set; } = false;
        public static bool RESOURCE_GARDEN { get; set; } = false;
        public static bool MANAGE_INVENTORY { get; set; } = false;

        public static bool ARENA { get; set; } = false;

        public static bool MELEE { get; set; } = true;
        public static bool CA_PRIMARTY { get; set; } = true;

        public static PlayerSettings Player {get;set;} = new PlayerSettings();

        public static String WINDOW_NAME { get; set; } = "Nodiatis";
        public static String SETTINGS_FILE { get; set; } = "Settings/settings.txt";

        public static bool isManagingInventory()
        {
            return !Settings.PILGRIMAGE && !Settings.BOSSING;
        }
    }
}
