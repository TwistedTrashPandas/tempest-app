using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerLeftSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Wind, Charge.Fire, Charge.Wind, Charge.Earth };
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
            return new SteerShip(SteerShip.SteeringDirection.Left, newSpellCast);
        }

        public override Color SpellColor
        {
            get
            {
                return new Color(166 / 255f, 77 / 255f, 255 / 255f);
            }
        }
    }
}
