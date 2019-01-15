using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SteerUpSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Water, Charge.Fire, Charge.Fire, Charge.Wind };
            }
        }

        public override String Name
        {
            get
            {
                return "Go UP!";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SteerShip(SteerShip.SteeringDirection.Up);
        }

        public override Color SpellColor
        {
            get
            {
                return new Color(255 / 255f, 210 / 255f, 77 / 255f);
            }
        }
    }
}
