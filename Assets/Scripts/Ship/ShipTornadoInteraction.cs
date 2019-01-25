using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ShipTornadoInteraction : MonoBehaviour
    {
        public float angularMomentumFactor = 1f;
        public float velocityDamp = 1f;
        public float pullStrength = 1f;

        private VectorField vectorField;
        private Rigidbody rb;
        private Vector3 targetView;
        private Vector3 targetForce;
        private Vector3 velDampVector;
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
                velDampVector = new Vector3(velocityDamp, 0.9f, velocityDamp);
            }
            else
                this.enabled = false;
        }

        private void FixedUpdate()
        {
            if (vectorField != null && rb != null)
            {
                // apply force depending on position in vectorfield and adjust viewing direction accordingly
                targetForce = vectorField.GetVectorAtPos(transform.position);
                targetForce.y = 0f;
                targetForce = targetForce * (1.0f - pullStrength) + pullStrength * (new Vector3(vectorField.GetCenterWS().x - transform.position.x, 0f, vectorField.GetCenterWS().z - transform.position.z)).normalized * targetForce.magnitude;
                targetView = rb.velocity.normalized;
                targetView.y /= 4f;
                rb.AddForce(targetForce);
                transform.LookAt(Vector3.Lerp(transform.forward, targetView, Time.fixedDeltaTime * angularMomentumFactor) + transform.position);

                targetView.y = 0f;
                Vector3 currForward = transform.forward;
                currForward.y = 0f;
                float angle = Vector3.SignedAngle(targetView, currForward, Vector3.up) / 1.5f;
                
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, angle), Time.fixedDeltaTime * 10f);

                rb.velocity.Scale(velDampVector);
            }
        }
    }
}