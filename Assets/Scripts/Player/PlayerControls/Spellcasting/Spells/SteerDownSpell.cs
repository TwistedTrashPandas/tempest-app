using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerDownSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Water, Charge.Fire, Charge.Earth, Charge.Earth };
            }
        }

        public override String Name
        {
            get
            {
                return "Down!";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Down, newSpellCast);
        }

        public override Color SpellColor
        {
            get
            {
                return new Color(77 / 255f, 121 / 255f, 255 / 255f);
            }
        }
    }
}
