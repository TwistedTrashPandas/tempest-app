using System;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public class SpellCastedEventArgs : EventArgs 
    {
        public Spell Spell {get; private set;}
        public SpellCastedEventArgs(Spell spell)
        {
            Spell = spell;
        }
    }
}
