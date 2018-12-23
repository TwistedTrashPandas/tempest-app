using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerRightSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get 
            {
                return new Rune[] {Rune.Fire, Rune.Water, Rune.Earth, Rune.Wind};
            }
        }

        public override String Name
        {
            get 
            {
                return "Left halfwind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Right);
        }
    }
}
