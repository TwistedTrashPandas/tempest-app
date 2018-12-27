using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.PlayerControls.Spellcasting;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    public class ChargingEventArgs : EventArgs
    {
        public Charge Charge { get; private set; }
        public ChargingEventArgs(Charge rune)
        {
            Charge = rune;
        }
    }
}
