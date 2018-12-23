using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls.Spellcasting;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class ChargingEventArgs : EventArgs
    {
        public Rune Rune { get; private set; }
        public ChargingEventArgs(Rune rune) 
        {
            Rune = rune;
        }
    }
}
