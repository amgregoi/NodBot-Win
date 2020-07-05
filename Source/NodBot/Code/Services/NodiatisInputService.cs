using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class NodiatisInputService
    {
        public InputService inputService { get;  private set; }
        public Logger logger { get; set; }

        public NodiatisInputService(Logger log)
        {
            logger = log;
            inputService = new InputService(Settings.WINDOW_NAME, this.logger);
        }

        public void Dispose()
        {
            inputService.Dispose();
        }

        public void moveUp()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.MOVE_UP);
        }

        public void moveDown()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.MOVE_DOWN);
        }

        public void moveLeft()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.MOVE_LEFT);
        }

        public void moveRight()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.MOVE_RIGHT);
        }

        public void GemSlot1()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.GEM_SLOT_1);
        }

        public void GemSlot2()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.GEM_SLOT_2);
        }

        public void GemSlot3()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.GEM_SLOT_3);
        }

        public void GemSlot4()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.GEM_SLOT_4);
        }
        public void GemSlot5()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.GEM_SLOT_5);
        }

        public void GemSlot6()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.GEM_SLOT_6);
        }
        public void AutoAttack()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.AUTO_ATTACK);
        }

        public void AutoShoot()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.AUTO_SHOOT);
        }

        public void PrimaryClassAbility()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.CA_PRIMARY);
        }

        public void SecondaryClassAbility()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.CA_SECONDARY);
        }

        public void SettingsAttack()
        {
            if (Settings.MELEE) AutoAttack();
            else AutoShoot();
        }

        public void SettingsClassAbility()
        {
            if (Settings.CA_PRIMARTY) PrimaryClassAbility();
            else SecondaryClassAbility();
        }

        public void Exit()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.EXIT);
        }

        public void LootTrophies()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.LOOT);
        }

        public void ClickOnPoint(int aX, int aY, bool aLeftClick)
        {
            inputService.sendLeftMouseClickWithWindowHandler(aX, aY, aLeftClick);
        }

        public void ClickOnPointNoMutex(int aX, int aY, bool aLeftClick)
        {
            inputService.moveMouse(aX, aY);
            inputService.doLeftClick();
            //inputService.doLeftClick(aX, aY, aLeftClick);
        }

        public void MoveCursorTo(int aX, int aY)
        {
            inputService.moveMouse(aX, aY);
        }

        public void InitiateFight()
        {
            inputService.sendKeyboardClick(InputService.Keyboard_Actions.START_FIGHT);
        }

    }
}
