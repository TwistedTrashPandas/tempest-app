﻿using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Damaging : EnvObject
    {
        public float damage;
        public float health;
        public DamagingStatus status;

        /*Rock Animation Code Starts*/
        public List<GameObject> rockParts;
        
        /*Rock Animation Code Ends*/

        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship" && collision.gameObject.GetComponent<Ship>().GetCurrenStatus().Condition != ShipCondition.Shielded)
            {/*
                Ship ship = collision.gameObject.GetComponentInParent<Ship>();
                ship.GetShipForceManipulator().AddForceAtPosition(collision.impulse, collision.contacts[0].point);*/
                ShipPart part = collision.collider.gameObject.GetComponent<ShipPart>();
                if(part != null)
                    part.ResolveCollision(damage, collision.contacts, collision.impulse);
                Explode(false);
            }
        }

        // call on server
        public void RemoveHealth(float h)
        {
            if (status == DamagingStatus.Fragile)
                Explode(false);
            else
                health -= h;
            if (health < 0)
                Explode(false);
        }

        // TODO: spawn new rocks?, explosion animation
        public void Explode(bool split)
        {
            if (!split)
                Destroy(this.gameObject);
            else
            {
                /*Rock Animation Code Starts*/
                //Get all seperate Objects
                foreach (Transform child in transform)
                {
                    rockParts.Add(child.gameObject);
                }
                rockParts.Add(this.transform.parent.gameObject);

                rockParts.ToArray();

                //Seperate those
                this.transform.parent = null;

                this.gameObject.AddComponent<RockAnimator>();


                /*Rock AnimationCode Ends*/

            }
        }
    }
}
