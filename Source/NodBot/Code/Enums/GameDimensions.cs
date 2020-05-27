using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Enums
{
    public enum GameDimension
    {
        WIDTH = 940,
        HEIGHT = 800,
        HEIGHT_NO_CHAT = 600,
        WIDTH_NO_SIDE_BAR = 800,
    }

    public static class GameDimensionExtension
    {
        public static int Value (this GameDimension section, int windowVal = -1)
        {
            if(windowVal < 0) return (int)section;

            if (windowVal < (int)section) return windowVal;
            
            return (int)section;
        }
    }
}
