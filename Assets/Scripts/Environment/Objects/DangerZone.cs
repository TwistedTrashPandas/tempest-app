using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class DangerZone : EnvObject
    {
        public DangerZoneType zoneType;

        protected override void OnTriggerEnter(Collider other)
        {
            switch (zoneType)
            {
                // handle interaction (e.g., set status of ship to freezing)
                case DangerZoneType.Freezing:
                    if (other.gameObject.tag == "Ship")
                    {
                        Ship ship = other.transform.parent.gameObject.GetComponent<Ship>();
                        ship.GetCurrenStatus().Condition |= ShipCondition.Freezing;
                    }
                    break;
                case DangerZoneType.Fragile:
                    if (other.gameObject.tag == "Ship")
                    {
                        ShipPart part = other.gameObject.GetComponent<ShipPart>();
                        part.status ^= ShipPartStatus.Fragile;
                    }
                    else if (other.GetComponent<Damaging>() != null)
                    {
                        other.GetComponent<Damaging>().status ^= DamagingStatus.Fragile;
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            switch (zoneType)
            {
                // handle interaction (e.g., set status of ship to normal)
                case DangerZoneType.Freezing:
                    if (other.gameObject.tag == "Ship")
                    {
                        Ship ship = other.transform.parent.gameObject.GetComponent<Ship>();
                        ship.GetCurrenStatus().Condition &= ~ShipCondition.Freezing;
                    }
                    break;
                case DangerZoneType.Fragile:
                    if (other.gameObject.tag == "Ship")
                    {
                        ShipPart part = other.gameObject.GetComponent<ShipPart>();
                        part.status ^= ShipPartStatus.Fragile;
                    }
                    else if(other.GetComponent<Damaging>() != null)
                    {
                        other.GetComponent<Damaging>().status ^= DamagingStatus.Fragile;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
