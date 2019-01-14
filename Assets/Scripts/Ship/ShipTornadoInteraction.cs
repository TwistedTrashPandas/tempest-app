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
                targetForce = targetForce * (1.0f - pullStrength) + pullStrength * (vectorField.GetCenterWS() - transform.position).normalized * targetForce.magnitude;
                targetForce.y = 0f;
                targetView = rb.velocity.normalized;
                targetView.y = 0f;
                rb.AddForce(targetForce);
                transform.LookAt(Vector3.Lerp(transform.forward, targetView, Time.fixedDeltaTime * angularMomentumFactor) + transform.position);
                rb.velocity *= velocityDamp;
            }
        }
    }
}