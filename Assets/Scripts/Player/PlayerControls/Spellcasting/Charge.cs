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
                case Charge.None: return "No charge";
                default: throw new InvalidOperationException($"Unexpected {nameof(Charge)} value of {Charge}");
            }
        }

        public static Color CorrespondingColor(this Charge Charge)
        {
            switch(Charge)
            {
                case Charge.Wind: return new Color(0.1243138f, 0.7509804f, 0.1529412f);
                case Charge.Fire: return new Color(0.6703922f, 0.12f, 0.12f);
                case Charge.Water: return new Color(0.1176471f, 0.127451f, 0.790196f);
                case Charge.Earth: return new Color(0.5607843f, 0.5156863f, 0.08235294f);
                case Charge.None: return Color.white;
                default: throw new InvalidOperationException($"Unexpected {nameof(Charge)} value of {Charge}");
            }
        }
    }
}
