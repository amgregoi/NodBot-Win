using NodBot.Code.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    class ArenaService
    {
        private NodiatisInputService nodInputService;
        private Logger mLogger;
        private ImageService imageService;
        private TimeService timeService;
        private SequenceStateCallback sequenceCallback;

        public ArenaService(ImageService image,NodiatisInputService input, Logger logger, SequenceStateCallback callback)
        {
            sequenceCallback = callback;

            imageService = image;
            mLogger = logger;
            nodInputService = input;
            timeService = new TimeService(logger);
        }

        public async Task StartCombat()
        {
            mLogger.sendMessage("Starting Attack", LogType.INFO);


            timeService.delay(500);
            if (!imageService.ContainsTemplateMatch(NodImages.Arena, screenSection: ScreenSection.Game)) return;

            // start auto attack [A/S]
            if (Settings.Player.isMelee)
            {
                nodInputService.AutoAttack();
            }
            else
            {
                nodInputService.AutoShoot();
            }

            sequenceCallback.setState(SequenceState.ATTACK);
        }

        public async Task EnterQueue()
        {
            sequenceCallback.setState(SequenceState.ARENA_WAIT_QUEUE);

            Random rand = new Random();

            int xOffset = rand.Next(500);
            int yOffset = rand.Next(150);

            int xInit = 200, yInit = 100;

            nodInputService.ClickOnPoint(xInit + xOffset, yInit + yOffset, false);

            timeService.delay(1000);
            xOffset += rand.Next(20, 50);
            nodInputService.ClickOnPoint(xInit + xOffset + rand.Next(20, 50), yInit + yOffset + 25, true);

            timeService.delay(1000);
            xOffset += rand.Next(20, 50);
            nodInputService.ClickOnPoint(xInit + xOffset + rand.Next(20, 50), yInit + yOffset + 72, true);

            timeService.delay(25);
        }
    }
}
