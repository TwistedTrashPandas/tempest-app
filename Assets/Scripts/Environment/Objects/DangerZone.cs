using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System;
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
                        if (ship != null)
                            ship.GetCurrenStatus().AddCondition(ShipCondition.Freezing);
                    }
                    break;
                case DangerZoneType.Fragile:
                    if (other.gameObject.tag == "Ship")
                    {
                        ShipPart part = other.gameObject.GetComponent<ShipPart>();
                        if (part != null)
                            part.status |= ShipPartStatus.Fragile;
                        Ship ship = other.transform.parent.gameObject.GetComponent<Ship>();
                        if (ship != null)
                            ship.GetCurrenStatus().AddCondition(ShipCondition.Fragile);
                    }
                    else if (other.GetComponentInParent<Damaging>() != null)
                    {
                        other.GetComponentInParent<Damaging>().status |= DamagingStatus.Fragile;
                    }
                    break;
                default: throw new InvalidOperationException($"Unexpected {nameof(DangerZoneType)} value of {zoneType}!");
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
                        if (ship != null)
                            ship.GetCurrenStatus().RemoveCondition(ShipCondition.Freezing);
                    }
                    break;
                case DangerZoneType.Fragile:
                    if (other.gameObject.tag == "Ship")
                    {
                        ShipPart part = other.gameObject.GetComponent<ShipPart>();
                        if (part != null)
                            part.status &= ~ShipPartStatus.Fragile;
                        Ship ship = other.transform.parent.gameObject.GetComponent<Ship>();
                        if (ship != null)
                            ship.GetCurrenStatus().RemoveCondition(ShipCondition.Fragile);
                    }
                    else if (other.GetComponentInParent<Damaging>() != null)
                    {
                        other.GetComponentInParent<Damaging>().status &= ~DamagingStatus.Fragile;
                    }
                    break;
                default: throw new InvalidOperationException($"Unexpected {nameof(DangerZoneType)} value of {zoneType}!");
            }
        }
    }
}
