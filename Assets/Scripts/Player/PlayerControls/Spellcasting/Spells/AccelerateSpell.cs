using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class AccelerateSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Forward);
        }
    }
}