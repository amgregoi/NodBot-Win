using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Enums
{
    public interface SequenceStateCallback
    {
        void setState(SequenceState state);
        SequenceState getState();
    }

    /// <summary>
    /// This is an enumerator defining the various combat states of the bot.
    /// 
    /// </summary>
    public enum SequenceState
    {
        BOT_START = 100,

        //Combat States
        WAIT = 0,
        WAIT_2 = 1,
        ATTACK = 2,
        END = 3,
        INIT = 4,

        //Arena States
        ARENA_WAIT_QUEUE = 20,
        ARENA_WAIT_COMBAT = 21,
        ARENA = 22,

        //TW States
        TOWN_WALKING = 30,

        //Resourcing
        RESOURCING = 40
    }
}
