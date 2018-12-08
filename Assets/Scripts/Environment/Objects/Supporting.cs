using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Supporting : EnvObject
    {
        public Vector3 forceDir;
        public float strength;

        private void Start()
        {
            if (forceDir != null)
                forceDir = Vector3.Normalize(forceDir);
            else
                forceDir = new Vector3(1, 0, 1);
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship")
            {
                Ship ship = collision.gameObject.GetComponent<Ship>();
                for(int i = 0; i< collision.contacts.Length;i++)
                    ship.GetShipForceManipulator().AddForceAtPosition(forceDir * strength, collision.contacts[i].point);
            }
        }
    }
}