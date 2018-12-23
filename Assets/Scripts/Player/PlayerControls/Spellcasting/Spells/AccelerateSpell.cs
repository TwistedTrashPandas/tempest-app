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
                return new Rune[] {Rune.Fire, Rune.Earth, Rune.Water, Rune.Water};
            }
        }

        public override String Name
        {
            get 
            {
                return "Tailwind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Forward);
        }
    }
}