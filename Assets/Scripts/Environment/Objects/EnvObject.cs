using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest
{
    namespace Environment
    {
        namespace Interacting
        {
            public class EnvObject : MonoBehaviour
            {

                protected Rigidbody rb;
                protected float speed;

                public EnvObject()
                {
                }

                public EnvObject(float sp)
                {
                    speed = sp;
                }

                private void Awake()
                {
                    rb = GetComponent<Rigidbody>();
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
    }
}