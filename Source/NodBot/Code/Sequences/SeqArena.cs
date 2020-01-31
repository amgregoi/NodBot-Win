using NodBot.Code.Enums;
using NodBot.Code.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class SeqArena : SeqBase
    {
        private ArenaService arenaService;
        private TimeService timeService;

        public SeqArena(CancellationTokenSource ct, Logger aLogger) : base(ct, aLogger)
        {
            timeService = new TimeService(aLogger);
            arenaService = new ArenaService(imageService, nodInputService, logger, this);
        }

        public override async Task Start()
        {
            combatState = SequenceState.ARENA;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (combatState >= SequenceState.ARENA_WAIT_QUEUE && imageService.ContainsMatch(NodImages.Arena, NodImages.CurrentSS))
                {
                    arenaService.StartCombat().Wait();
                }
                else if (combatState >= SequenceState.ARENA_WAIT_COMBAT && imageService.ContainsMatch(NodImages.Dust, NodImages.CurrentSS))
                {
                    arenaService.EnterQueue().Wait();;
                }
                else
                {
                    timeService.delay(1500); // wait 1.5s
                }
            }            
        }
    }
}
