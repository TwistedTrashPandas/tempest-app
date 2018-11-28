using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class DangerZone : EnvObject
    {
        protected override void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.tag == "Ship")
            {
                // handle interaction
            }
        }
    }
}
