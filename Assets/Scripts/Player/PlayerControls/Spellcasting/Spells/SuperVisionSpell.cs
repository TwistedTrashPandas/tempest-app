using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SuperVisionSpell : Spell
    {
        public override Charge[] SpellSequence
        {
            get
            {
                return new Charge[] { Charge.Wind, Charge.Earth, Charge.Wind, Charge.Wind };
            }
        }

        public override String Name
        {
            get
            {
                return "Arcane insight";
            }
        }

        public override PlayerAction GetPlayerAction()
        {
            return new SuperVision();
        }
    }
}
