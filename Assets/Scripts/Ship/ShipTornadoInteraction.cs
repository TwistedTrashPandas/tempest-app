using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipTornadoInteraction : MonoBehaviour
    {
        public bool linearMovement;

        public float angularMomentumFactor = 1f;
        public float velocityDamp_xz = 0.99f;
        public float velocityDamp_y = 0.9f;
        public float pullStrength = 1f;
        public float maximumVelocity;
        public new float constantForce;

        private VectorField vectorField;
        private Rigidbody rb;
        private Vector3 targetView;
        private Vector3 shipTargetForce;
        private Vector3 velDampVector;
        private Vector3 tornCenter;
        // Start is called before the first frame update
        void Start()
        {
            if (GetComponent<ServerObject>().onServer)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                    throw new System.InvalidOperationException("A rigidbody has to be attached to the ship.");
                vectorField = GameObject.Find("EnvironmentManager").GetComponent<VectorField>();
                if (vectorField == null)
                    throw new System.InvalidOperationException("EnvironmentManager has to be in the same scene as the ship.");
                targetView = transform.forward;
                velDampVector = new Vector3(velocityDamp_xz, velocityDamp_y, velocityDamp_xz);
                tornCenter = vectorField.GetCenterWS();
                shipTargetForce = new Vector3();
            }
            else
                this.enabled = false;
        }

        private void FixedUpdate()
        {
            if (vectorField != null && rb != null)
            {
                if (!linearMovement)
                {
                    // apply force depending on position in vectorfield and adjust viewing direction accordingly
                    shipTargetForce = vectorField.GetVectorAtPos(transform.position);
                    shipTargetForce.y = 0f;
                    shipTargetForce = shipTargetForce * (1.0f - pullStrength) + pullStrength * new Vector3(tornCenter.x - transform.position.x, 0f, tornCenter.z - transform.position.z).normalized * shipTargetForce.magnitude;
                }
                else
                {
                    shipTargetForce = transform.forward;
                    shipTargetForce.y = 0f;
                    rb.AddForce(shipTargetForce.normalized * constantForce);
                }

                rb.velocity.Scale(velDampVector);
            }
        }

        private void Update()
        {

            if (vectorField != null && rb != null)
            {
                if (!linearMovement || true)
                {
                    // adjusting orientation of the ship depending on movement
                    targetView = rb.velocity.normalized;
                    targetView.y /= 4f;


                    if (Vector3.Dot(transform.forward, rb.velocity.normalized) >= -0.1f)
                    {
                        transform.LookAt(Vector3.Lerp(transform.forward, targetView, Time.fixedDeltaTime * angularMomentumFactor) + transform.position);
                    }

                    if (Vector3.Dot(transform.forward, rb.velocity.normalized) >= -0.1f)
                    {
                        targetView.y = 0f;
                        Vector3 currForward = transform.forward;
                        currForward.y = 0f;

                        float angle = Vector3.SignedAngle(targetView, currForward, Vector3.up) / 1.5f;
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, angle), Time.fixedDeltaTime * 15f);
                    }
                }
            }
        }
    }
}