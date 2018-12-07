using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipStatus
    {
        public float Health { get; set; }
        public float Shield { get; set; }
        public ShipCondition Condition { get; set; }
    }
}