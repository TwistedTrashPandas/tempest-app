using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Damaging : EnvObject
    {
        public float damage;

        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship")
            {/*
                Ship ship = collision.gameObject.GetComponentInParent<Ship>();
                ship.GetShipForceManipulator().AddForceAtPosition(collision.impulse, collision.contacts[0].point);*/
                collision.collider.gameObject.GetComponent<ShipPart>().ResolveCollision(damage, collision.contacts, collision.impulse);
                Explode(false);
            }
        }

        // TODO: spawn new rocks?, explosion animation
        public void Explode(bool split)
        {
            if (!split)
                Destroy(this.gameObject);
            else
            {

            }
        }
    }
}
