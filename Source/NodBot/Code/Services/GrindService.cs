using NodBot.Code.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    public interface GrindCallback : SequenceStateCallback
    {
        void updateKillCounter();
        void updateChestCounter();
    }

    public class GrindService
    {
        private NodiatisInputService nodInput;
        private Logger logger;
        private ImageService imageService;
        private TimeService timeService;
        private InventoryService inventoryService;
        private GrindCallback grindCallback;

        private int killCounter = 0;

        public GrindService(ImageService image, NodiatisInputService input, Logger log, GrindCallback callback)
        {
            imageService = image;
            logger = log;
            nodInput = input;
            timeService = new TimeService(this.logger);
            grindCallback = callback;

            //setupInventoryService();
            inventoryService = InventoryService.Instance;
        }

        /// <summary>
        /// This function initiates combat from the over world map.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task enterCombat()
        {
            grindCallback.setState(SequenceState.INIT);

            logger.info("Starting Combat");
            nodInput.InitiateFight();
            timeService.delay(750);

            //increment kill count
            killCounter++;
            grindCallback.updateKillCounter();
        }

        /// <summary>
        /// This function starts attacking and uses the players class ability, both specified
        /// by the UI settings options.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task startCombat()
        {
            // If bot is started and already in combat, skip starting combat
            if (grindCallback.getState() == SequenceState.BOT_START)
            {
                grindCallback.setState(SequenceState.ATTACK);
                logger.debug("Already in combat");
                return;
            }

            logger.info("Starting Attack");
            timeService.delay(TimeService.OffsetLength.Short);

            // start auto attack [A/S]
            if (Settings.Player.isMelee || Settings.Player.magic.usesStaff)
            {
                // TODO :: Verify  this autoshoot function works
                if (Settings.Player.startCombatRangeSwapMelee)
                {
                    nodInput.AutoShoot();
                    timeService.delay(2000, TimeService.OffsetLength.Medium);
                }
                nodInput.AutoAttack();
            }
            else
            {
                nodInput.AutoShoot();
            }

            timeService.delay(1500, TimeService.OffsetLength.Medium);

            // start class ability [D/F]
            if (Settings.Player.usePrimaryClassAbility)
            {
                nodInput.PrimaryClassAbility();
            }
            else if (Settings.Player.useSecondaryClassAbility)
            {
                nodInput.SecondaryClassAbility();
            }

            // long wait at start of combat
            //await delay (20000 + generateOffset(15000));
            grindCallback.setState(SequenceState.ATTACK);

            if (Settings.MANAGE_INVENTORY)
            {
                _ = Task.Run(() =>
                {
                    if (inventoryService != null) inventoryService.SortInventory();
                });
            }
        }

        /// <summary>
        /// This function loots trophies, scans for chests, and exits combat.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task EndCombat()
        {
            // In case bot is started at end of combat, it will still attempt to loot
            if (grindCallback.getState() == SequenceState.BOT_START)
                grindCallback.setState(SequenceState.WAIT_2);

            logger.info("Ending combat");

            if (!Settings.PILGRIMAGE)
            {
                // loot trophies
                if (!Settings.BOSSING) // SKIP loot if bossing
                {
                    logger.info("Lotting trophies");
                    nodInput.LootTrophies();
                }
                else logger.info("Skipped looting trophies?");
                
                timeService.delay(2000, TimeService.OffsetLength.Medium);

                // loot chest
                if (Settings.CHESTS)
                {
                    logger.info("Starting search for chests.");
                    Point? coord = imageService.FindChestCoord();
                    if (coord != null)
                    {
                        nodInput.ClickOnPoint(coord.Value.X, coord.Value.Y, true);
                        grindCallback.updateChestCounter();
                    }
                    timeService.delay(2000, TimeService.OffsetLength.Medium);
                }
                else logger.info("Skipped chests?");
            }
            else
            {
                logger.debug("Pilgrimage Active, skipping trophies and chests.");
                timeService.delay(250, TimeService.OffsetLength.Short);
            }

            // exit combat
            nodInput.Exit();

            //update combat state
            grindCallback.setState(SequenceState.END);

            // Check if we should take break after some random range of kills
            if(timeService.takeBreak(killCounter))
            {
                // reset kill counter
                killCounter = 0;
            }
        }
    }
}
