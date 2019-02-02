using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Environment.Interacting
{
    public class EnvObject : MonoBehaviour
    {
        public int listIndex;
        public Vector3 relativeTargetPos;
        public EnvObjectType type;
        public float speed;
        public float closestDistance; // distance until which the objects will pursue the target position
        public MoveType moveType;
        private bool pastShip;
        private Vector3 lastDirection;
        protected new Rigidbody rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
                throw new System.InvalidOperationException("EnvObject cannot operate without Rigidbody on the same object.");
            closestDistance *= closestDistance;
        }

        public void MoveNext(Vector3 targetPos, Vector3 vectorVal)
        {
            vectorVal.y = 0f;
            switch (moveType)
            {
                case MoveType.Direct:
                    MoveDirectly(targetPos);
                    break;
                case MoveType.Force:
                    AddForce(vectorVal, new Vector3());
                    break;
                case MoveType.Velocity:
                    SetVelocity(vectorVal);
                    break;
                case MoveType.ForceDirect:
                    MoveDirectly(targetPos);
                    AddForce(vectorVal, new Vector3());
                    break;
                case MoveType.Static:
                    break;
                default:
                    throw new System.InvalidOperationException("MoveType of Environment Spawner has to be set");
            }
        }

        public void MoveDirectly(Vector3 targetPos)
        {
            if (!pastShip)
            {
                if (Vector3.SqrMagnitude(transform.position - targetPos) > closestDistance)
                {
                    AddForce((targetPos + relativeTargetPos - transform.position).normalized * speed, Vector3.zero);
                }
                   // rigidbody.MovePosition(transform.position + (targetPos + relativeTargetPos - transform.position).normalized * speed * Time.fixedDeltaTime);
                else
                {
                    pastShip = true;
                    lastDirection = (targetPos + relativeTargetPos - transform.position).normalized;
                }
            }
            else
            {
                AddForce(lastDirection * speed, Vector3.zero);
                //rigidbody.MovePosition(transform.position + lastDirection * speed * Time.fixedDeltaTime);
            }
        }

        public void SetMass(float mass)
        {
            rigidbody.mass = mass;
        }

        public void ClampVelocity(float maxVel)
        {
            if (rigidbody.velocity.magnitude > maxVel)
                rigidbody.velocity = rigidbody.velocity.normalized * maxVel;
        }

        public void MoveRigidbodyTo(Vector3 pos)
        {
            rigidbody.MovePosition(pos);
        }

        public void RotateRigidbodyTo(Quaternion rot)
        {
            rigidbody.MoveRotation(rot);
        }

        public void SetVelocity(Vector3 v)
        {
            rigidbody.velocity = v;
        }

        public void AddForce(Vector3 force, Vector3 pos)
        {
            //rigidbody.AddForceAtPosition(force, transform.position);
            rigidbody.AddForce(force * rigidbody.mass);
        }

        public void DampVelocity(float damping_factor)
        {
            rigidbody.velocity *= damping_factor;
        }

        public void DampForce(float damping_factor)
        {
            rigidbody.AddForce(-damping_factor * rigidbody.velocity * rigidbody.mass);
        }

        public void EnableGravity()
        {
            rigidbody.useGravity = true;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {

        }
        protected virtual void OnCollisionStay(Collision collision)
        {

        }

        protected virtual void OnTriggerEnter(Collider other)
        {

        }
        protected virtual void OnTriggerStay(Collider other)
        {

        }

        protected virtual void OnTriggerExit(Collider other)
        {

        }
    }
}