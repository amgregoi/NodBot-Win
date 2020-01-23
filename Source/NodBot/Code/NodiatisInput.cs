﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class NodiatisInput
    {
        public Input mInputController { get;  private set; }
        public Logger LOG { get; set; }

        public NodiatisInput(Logger aLogger)
        {
            LOG = aLogger;
            mInputController = new Input(Settings.WINDOW_NAME, LOG);
        }

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

        public void GemSlot1()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.GEM_SLOT_1);
        }

        public void GemSlot2()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.GEM_SLOT_2);
        }

        public void GemSlot3()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.GEM_SLOT_3);
        }

        public void GemSlot4()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.GEM_SLOT_4);
        }
        public void GemSlot5()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.GEM_SLOT_5);
        }

        public void GemSlot6()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.GEM_SLOT_6);
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

        public void ClickOnPoint(int aX, int aY, bool aLeftClick)
        {
            mInputController.sendLeftMouseClickWithWindowHandler(aX, aY, aLeftClick);
        }

        public void MoveCursorTo(int aX, int aY)
        {
            mInputController.moveMouse(aX, aY);
        }

        public void InitiateFight()
        {
            mInputController.sendKeyboardClick(Input.Keyboard_Actions.START_FIGHT);
        }

    }
}
