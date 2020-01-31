using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    class MagicService
    {
        private NodiatisInputService mInput;
        private GemSlot currentGemSlot = GemSlot.Five;

        public MagicService(NodiatisInputService input)
        {
            mInput = input;
        }

        enum GemSlot
        {
            One, Two, Three, Four, Five, Six
        }

        /// <summary>
        /// 
        /// </summary>
        public void UseGemSlot()
        {
            switch (currentGemSlot)
            {
                case GemSlot.One:
                    mInput.GemSlot1();
                    break;
                case GemSlot.Two:
                    mInput.GemSlot2();
                    break;
                case GemSlot.Three:
                    mInput.GemSlot3();
                    break;
                case GemSlot.Four:
                    mInput.GemSlot4();
                    break;
                case GemSlot.Five:
                    mInput.GemSlot5();
                    break;
                case GemSlot.Six:
                    mInput.GemSlot6();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateNextGemSlot()
        {
            if (Settings.Player.magic.cycleAllGemSlots)
            {
                switch (currentGemSlot)
                {
                    case GemSlot.One:
                        currentGemSlot = GemSlot.Two;
                        break;
                    case GemSlot.Two:
                        currentGemSlot = GemSlot.Three;
                        break;
                    case GemSlot.Three:
                        currentGemSlot = GemSlot.Four;
                        break;
                    case GemSlot.Four:
                        currentGemSlot = GemSlot.Five;
                        break;
                    case GemSlot.Five:
                        if (!Settings.Player.magic.cycleAllGemSlots) currentGemSlot = GemSlot.Five;
                        else if (!Settings.Player.magic.usesStaff) currentGemSlot = GemSlot.Six;
                        else if (Settings.Player.magic.isPremium) currentGemSlot = GemSlot.One;
                        else if (Settings.Player.magic.isStandard) currentGemSlot = GemSlot.Two;
                        else currentGemSlot = GemSlot.Three;
                        break;
                    case GemSlot.Six:
                        if (Settings.Player.magic.isPremium) currentGemSlot = GemSlot.One;
                        else if (Settings.Player.magic.isStandard) currentGemSlot = GemSlot.Two;
                        else currentGemSlot = GemSlot.Three;
                        break;
                }
            }
        }
    }
}
