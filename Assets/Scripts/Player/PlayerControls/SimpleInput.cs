using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Listens to user's keyboard input and sends a <see cref="MessageAction"/> if player pressed 3 same keys in a row
    /// </summary>
    public class SimpleInput : PlayerInputController
    {
        private Array keyCodes;
        private KeyCode lastPressed;
        private int counter;
        private const int triggerAmount = 3;

        private void Awake()
        {
            keyCodes = Enum.GetValues(typeof(KeyCode));
        }

        public override void Interrupt()
        {
            throw new NotImplementedException();
        }

        public override void Resume()
        {
            throw new NotImplementedException();
        }

        public override void Suppress()
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            foreach (KeyCode vKey in keyCodes)
            {
                if (Input.GetKeyDown(vKey))
                {
                    if (lastPressed == vKey)
                    {
                        ++counter;
                    }
                    else
                    {
                        counter = 1;
                        lastPressed = vKey;
                    }
                }
            }
            if (counter == triggerAmount)
            {
                TriggerActionEvent(new ActionMadeEventArgs(new MessageAction()));
                counter = 0;
            }
        }
    }
}
