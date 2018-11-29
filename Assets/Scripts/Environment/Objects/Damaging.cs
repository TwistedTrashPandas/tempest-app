using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Damaging : EnvObject
    {
        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship")
            {
                Ship ship = collision.gameObject.GetComponent<Ship>();
                ship.GetShipForceManipulator().AddForceAtPosition(collision.impulse, collision.contacts[0].point);
            }
        }

        // TODO: event for envspawner -> remove from list
        public void Explode()
        {
            
        }
    }
}
