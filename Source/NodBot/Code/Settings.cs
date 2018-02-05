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

        public static bool ARENA { get; set; } = false;

        public static bool MELEE { get; set; } = true;
        public static bool CA_PRIMARTY { get; set; } = true;
    }
}
