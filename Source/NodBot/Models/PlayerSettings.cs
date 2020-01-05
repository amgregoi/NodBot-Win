using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Model
{
    public class PlayerSettings
    {
        public String playerName { get; set; } = "DTM";

        // if both false, bot will not attack
        public bool isMelee { get; set; } = true;
        public bool isRange { get; set; }
        public bool useMagic { get; set; }

        public bool startCombatRangeSwapMelee { get; set; }

        // if both false, bot will skip class ability at start of combat
        public bool usePrimaryClassAbility { get; set; } = true;
        public bool useSecondaryClassAbility { get; set; }

        public MagicSettings magic { get; set; } = new MagicSettings();

    }
}

/**
 * Example Json
 { 
   "playerName":"Pally",
   "isMelee":false,
   "isRange":false,
   "useMagic":false,
   "usePrimaryClassAbility":false,
   "useSecondaryClassAbility":false,
   "magic":{ 
      "usesStaff":false,
      "cycleAllGemSlots":false,
      "isStandard":false,
      "isPremium":false,
      "usesAuras":false
   }
}
 */
