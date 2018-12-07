using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SlowdownSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get
            {
                return new Rune[] {Rune.Fire, Rune.Fire, Rune.Ice, Rune.Water};
            }
        }

        public override String Name
        {
            get 
            {
                return "Headwind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Backward);
        }
    }
}
