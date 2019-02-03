using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerHardRightSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Fire, Charge.Water, Charge.Fire, Charge.Wind };
            }
        }

        public override String Name
        {
            get
            {
                return "Left wind";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.HardRight, newSpellCast);
        }

        public override Color SpellColor
        {
            get
            {
                return new Color(255f / 255f, 255 / 255f, 144f / 255f);
            }
        }
    }
}
