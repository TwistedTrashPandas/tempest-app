using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SuperVisionSpell : Spell
    {
        public override Rune[] SpellSequence
        {
            get
            {
                return new Rune[] { Rune.Wind, Rune.Ice, Rune.Wind, Rune.Wind };
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
