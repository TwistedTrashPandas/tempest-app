using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Supporting : EnvObject
    {
        private Vector3 forceDir;
        private float strength;

        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship")
            {
                Ship ship = collision.gameObject.GetComponent<Ship>();
                ship.GetShipForceManipulator().AddForceAtPosition(collision.impulse, collision.contacts[0].point);
            }
        }
    }
}