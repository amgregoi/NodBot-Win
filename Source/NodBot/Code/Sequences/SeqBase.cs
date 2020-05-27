using NodBot.Code.Enums;
using NodBot.Code.Services;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public abstract class SeqBase : SequenceStateCallback
    {
        protected CancellationTokenSource tokenSource;
        protected CancellationToken token;

        protected ImageService imageService;
        protected NodiatisInputService nodInputService;
        protected InventoryService inventoryService;
        protected Logger logger;

        protected SequenceState combatState;

        protected SeqBase(CancellationTokenSource ct, Logger aLogger)
        {
            combatState = SequenceState.BOT_START;

            tokenSource = ct;
            token = ct.Token;

            logger = aLogger;
            nodInputService = new NodiatisInputService(logger);
            imageService = new ImageService(tokenSource, logger, nodInputService.inputService.getGameWindow());
            inventoryService = InventoryService.Instance;
        }

        public virtual async Task Start()
        {
            logger.sendLog("Base Method NOT IMPLEMENTED", LogType.INFO);
            await Task.Yield();
        }

        /***
         * 
         * SequenceStateCallback definitions
         * 
         */

        public void setState(SequenceState state)
        {
            combatState = state;
        }

        public SequenceState getState()
        {
            return combatState;
        }
    }
}
