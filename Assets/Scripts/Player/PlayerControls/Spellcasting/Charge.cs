using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    public enum Charge
    {
        None = 0,
        Wind = 1,
        Fire = 2,
        Water = 3,
        Earth = 4
    }

    public static class ChargeExtensions
    {
        public static string FriendlyName(this Charge Charge)
        {
            switch(Charge)
            {
                case Charge.Wind: return "Air";
                case Charge.Fire: return "Fire";
                case Charge.Water: return "Water";
                case Charge.Earth: return "Earth";
                default: throw new InvalidOperationException($"Unexpected {nameof(Charge)} value of {Charge}");
            }
        }
    }
}
