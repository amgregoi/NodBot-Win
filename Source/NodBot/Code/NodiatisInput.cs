using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class NodiatisInput
    {
        private Input mInputController;

        public NodiatisInput(Logger aLogger)
        {
            LOG = aLogger;
            mInputController = new Input("Nodiatis", LOG);
        }

        public Logger LOG { get; set; }

        public void moveUp()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.MOVE_UP);
        }

        public void moveDown()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.MOVE_DOWN);
        }

        public void moveLeft()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.MOVE_LEFT);
        }

        public void moveRight()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.MOVE_RIGHT);
        }

        public void AutoAttack()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.AUTO_ATTACK);
        }

        public void AutoShoot()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.AUTO_SHOOT);
        }

        public void PrimaryClassAbility()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.CA_PRIMARY);
        }

        public void SecondaryClassAbility()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.CA_SECONDARY);
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
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.EXIT);
        }

        public void LootTrophies()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.LOOT);
        }

        public void ClickOnPoint(int aX, int aY)
        {
            mInputController.moveMouse(aX, aY);
        }

        public void InitiateFight()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.START_FIGHT);
        }

    }
}
