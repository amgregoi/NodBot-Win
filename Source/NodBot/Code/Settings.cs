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
        public static bool WAIT_FOR_RESOURCES { get; set; } = false;

        public static bool ARENA { get; set; } = false;

        public static bool MELEE { get; set; } = true;
        public static bool CA_PRIMARTY { get; set; } = true;

        public static PlayerSettings Player {get;set;} = new PlayerSettings();

        public static String WINDOW_NAME { get; set; } = "Nodiatis";
        public static String SETTINGS_FILE { get; set; } = "Settings/settings.txt";
        public static String ZONE { get; set; } = "NDreadPass";

        public static bool isManagingInventory()
        {
            return !Settings.PILGRIMAGE && !Settings.BOSSING;
        }
    }
}
