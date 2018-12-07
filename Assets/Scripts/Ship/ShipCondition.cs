using System;

namespace MastersOfTempest.ShipBL
{
    /// <summary>
    /// Describes the current condition effects acting on the Ship
    /// Can have multiple conditions present at the same time
    /// </summary>
    [Flags]
    public enum ShipCondition
    {
        None = 0,
        NoSpells = 1,
        Freezing = 2,
        Shielded = 4
    }
}
