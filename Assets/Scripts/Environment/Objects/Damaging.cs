using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class Damaging : EnvObject
    {
        public float damage;
        public float health;
        public float splitForce;
        public DamagingStatus status;
        public EnvSpawner envSpawner;

        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ship" && collision.gameObject.GetComponent<Ship>().GetCurrenStatus().Condition != ShipCondition.Shielded)
            {/*
                Ship ship = collision.gameObject.GetComponentInParent<Ship>();
                ship.GetShipForceManipulator().AddForceAtPosition(collision.impulse, collision.contacts[0].point);*/
                ShipPart part = collision.collider.gameObject.GetComponent<ShipPart>();
                if (part != null)
                    part.ResolveCollision(damage, collision.contacts, collision.impulse);
                Explode(false);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Water"))
                Destroy(this.gameObject);
        }

        // call on server
        public void RemoveHealth(float h)
        {
            if (status == DamagingStatus.Fragile)
                Explode(false);
            else
                health -= h;
            if (health < 0)
                Explode(true);
        }
        
        public void Explode(bool split)
        {
            if (!split)
                Destroy(this.gameObject);
            else
            {
                /*Rock Animation Code Starts*/
                GameObject[] children = transform.GetComponentsInChildren<GameObject>();

                for (int i = 1; i < children.Length; i++)
                {
                    // TODO change rigidbody parameters
                    GameObject currentRockPart = GameObject.Instantiate(this.gameObject);
                    GameObject[] children_2 = currentRockPart.transform.GetComponentsInChildren<GameObject>();
                    for (int j = 1; j < children_2.Length; j++)
                    {
                        if (i != j)
                            children_2[i].SetActive(false);
                    }
                    currentRockPart.GetComponent<Rigidbody>().AddForce(splitForce * new Vector3(UnityEngine.Random.Range(0.5f, 2f), UnityEngine.Random.Range(0.5f, 2f), UnityEngine.Random.Range(0.5f, 2f)));
                    envSpawner.AddEnvObject(currentRockPart.GetComponent<Damaging>());
                }
                Destroy(this.gameObject);
                /*Rock AnimationCode Ends*/
            }
        }
    }
}
