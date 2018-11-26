using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public abstract class EnvObject : MonoBehaviour
    {
        Rigidbody rb;
        public int instanceID { get; set; }
        public EnvSpawner.EnvObjectType type;

        public EnvObject()
        {
        }

        public EnvObject(float sp)
        {
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                throw new System.InvalidOperationException("EnvObject cannot operate without Rigidbody on the same object.");
        }

        public void SetVelocity(Vector3 v)
        {
            rb.velocity = v;
        }

        public void AddForce(Vector3 force, Vector3 pos)
        {
            rb.AddForceAtPosition(force, pos);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
        }
    }
}