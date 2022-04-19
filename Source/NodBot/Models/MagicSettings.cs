using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Model
{
    public class MagicSettings
    {
        // Will skip slot 6 to allow preset gem to stay in play
        public bool usesStaff { get; set; }
        // Will use just gem slot 5 otherwise
        public bool cycleAllGemSlots { get; set; }

        // Will activate gem slot 2
        public bool isStandard { get; set; }
        // Will activate gem slot 1
        public bool isPremium { get; set; }

        // Will skip gem slots 3,4,5 if gem cycling is active to keep auras in play
        public bool usesAuras { get; set; }

        public bool dcdd = true;
    }
}
