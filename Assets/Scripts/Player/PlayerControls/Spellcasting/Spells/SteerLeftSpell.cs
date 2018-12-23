using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerLeftSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get
            {
                return new Rune[] {Rune.Earth, Rune.Fire, Rune.Water, Rune.Wind};
            }
        }

        public override String Name
        {
            get 
            {
                return "Right halfwind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Left);
        }
    }
}
