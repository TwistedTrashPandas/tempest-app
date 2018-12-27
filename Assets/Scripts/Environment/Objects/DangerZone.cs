using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class DangerZone : EnvObject
    {
        protected override void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Ship")
            {
                // handle interaction (e.g., set status of ship to freezing)
                Ship ship = other.transform.parent.gameObject.GetComponent<Ship>();
                ship.GetCurrenStatus().Condition = ShipCondition.Freezing;
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Ship")
            {
                // handle interaction (e.g., set status of ship to freezing)
                Ship ship = other.transform.parent.gameObject.GetComponent<Ship>();
                ship.GetCurrenStatus().Condition = ShipCondition.None;
            }
        }
    }
}
