using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public abstract class EnvObject : MonoBehaviour
    {
        public int instanceID { get; set; }
        public EnvSpawner.EnvObjectType type { get; set; }
        private new Rigidbody rigidbody;
        
        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
                throw new System.InvalidOperationException("EnvObject cannot operate without Rigidbody on the same object.");
        }

        public void SetVelocity(Vector3 v)
        {
            rigidbody.velocity = v;
        }

        public void AddForce(Vector3 force, Vector3 pos)
        {
            rigidbody.AddForceAtPosition(force, rigidbody.transform.position);
        }

        public void DampVelocity(float damping_factor)
        {
            rigidbody.velocity *= damping_factor;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
        }
    }
}