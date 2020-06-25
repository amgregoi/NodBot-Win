using Emgu.CV;
using Emgu.CV.Structure;
using NodBot.Code.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Enums
{
    public enum ScreenSection
    {
        All, Game, Inventory, Storage, MiniGame, MiniGameNoOffset
    }

    public static class ScreenSectionExtension
    {
        private static int XOffset = 20;
        private static int YOffset = 40;
        public static Rectangle getSubRect(this ScreenSection section, Image<Bgr, byte> source)
        {
            Rectangle result;
            switch (section)
            {
                case ScreenSection.Game:
                    result = new Rectangle(XOffset, YOffset, GameDimension.WIDTH.Value(windowVal: source.Width) - XOffset, GameDimension.HEIGHT.Value(windowVal: source.Height) - YOffset);
                    break;
                case ScreenSection.MiniGameNoOffset:
                    result = new Rectangle(0, 0, GameDimension.WIDTH_NO_SIDE_BAR.Value(windowVal: source.Width), GameDimension.HEIGHT_NO_CHAT.Value(windowVal: source.Height));
                    break;
                case ScreenSection.MiniGame:
                    result = new Rectangle(XOffset, YOffset, GameDimension.WIDTH_NO_SIDE_BAR.Value(windowVal: source.Width) - XOffset, GameDimension.HEIGHT_NO_CHAT.Value(windowVal: source.Height) - YOffset);
                    break;
                case ScreenSection.Inventory:
                    result = new Rectangle(XOffset + GameDimension.WIDTH.Value(windowVal: source.Width), YOffset, source.Width - XOffset - GameDimension.WIDTH.Value(windowVal: source.Width), GameDimension.HEIGHT.Value(windowVal: source.Height) - YOffset);
                    break;
                case ScreenSection.Storage:
                    result = new Rectangle(0, GameDimension.HEIGHT.Value(), source.Width, source.Height - GameDimension.HEIGHT.Value(windowVal: source.Height));
                    break;
                case ScreenSection.All:
                default:
                    result = new Rectangle(0, 0, source.Width, source.Height);
                    break;
            }

            return result;
        }

        public static int getYOffset(this ScreenSection section)
        {
            switch (section)
            {
                case ScreenSection.Storage:
                    return GameDimension.HEIGHT.Value();
                case ScreenSection.Game:
                case ScreenSection.MiniGame:
                case ScreenSection.Inventory:
                    return YOffset;
                case ScreenSection.All:
                default:
                    return 0;
            }
        }


        public static int getXOffset(this ScreenSection section)
        {
            switch (section)
            {
                case ScreenSection.Inventory:
                    return GameDimension.WIDTH.Value() + XOffset;
                case ScreenSection.Game:
                case ScreenSection.MiniGame:
                    return XOffset;
                case ScreenSection.All:
                case ScreenSection.Storage:
                default:
                    return 0;
            }
        }
    }
}
