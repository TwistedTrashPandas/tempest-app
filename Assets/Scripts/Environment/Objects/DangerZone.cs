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
                // handle interaction (e.g., set status of ship to freezing)
                Ship ship = collision.gameObject.GetComponent<Ship>();
                ship.GetCurrenStatus().Condition = ShipCondition.Freezing;
            }
        }
    }
}
