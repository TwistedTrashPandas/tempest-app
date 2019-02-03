using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SlowdownSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Earth, Charge.Water, Charge.Earth, Charge.Wind };
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
            return new SteerShip(SteerShip.SteeringDirection.Backward, newSpellCast);
        }

        public override Color SpellColor
        {
            get
            {
                return new Color(255 / 255f, 77 / 255f, 77 / 255f);
            }
        }
    }
}
