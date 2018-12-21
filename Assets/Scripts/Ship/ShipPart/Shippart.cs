using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using MastersOfTempest.Networking;
using System;

namespace MastersOfTempest.ShipBL
{
    public class ShipPart : NetworkBehaviour
    {
        public int interactionArea;
        public float cutOffDist = 20f;
        public float impulseScaling = 0.5f;

        /// <summary>
        /// destruction == 0:   ship part fully repaired
        ///             == 1:   ship part fully destroyed
        /// </summary>
        private float destruction;
        private Vector3[] initialMesh;
        private Vector3[] targetMesh;

        protected override void Start()
        {
            base.Start();
            initialMesh = GetComponent<MeshFilter>().mesh.vertices;
            targetMesh = initialMesh;
            if (initialMesh == null)
                throw new System.InvalidOperationException("Ship part can only be attached to objects with meshes");
        }

        public float GetDestruction()
        {
            return destruction;
        }

        public void ResolveCollision(float destruc, ContactPoint[] contactPoints, Vector3 impulse)
        {
            UpdateMesh(contactPoints, impulse);
            AddDestruction(destruc);
        }

        public void UpdateMesh(ContactPoint[] contactPoints, Vector3 impulse)
        {
            Vector3[] currVerts = GetComponent<MeshFilter>().mesh.vertices;

            byte[] buffer = new byte[currVerts.Length * 12];

            for (int i = 0; i < contactPoints.Length; i++)
            {
                Vector3 currContact = contactPoints[i].point;
                //Vector3 normal = contactPoints[i].normal;
                for (int j = 0; j < currVerts.Length; j++)
                {
                    Vector3 worldPos = transform.TransformPoint(currVerts[j]);
                    float distSquared = Vector3.Distance(worldPos, currContact);
                    if (cutOffDist > distSquared)
                    {
                        Vector3 dir = (worldPos - currContact).normalized;
                        if (Vector3.Dot(dir, impulse.normalized) > 0)
                            dir *= impulse.magnitude;
                        else
                            dir = impulse;
                        distSquared *= distSquared;
                        worldPos += dir / (distSquared + 1f) / contactPoints.Length * impulseScaling;
                    }
                    currVerts[j] = transform.InverseTransformPoint(worldPos);
                }
            }
            for (int j = 0; j < currVerts.Length; j++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(currVerts[j].x), 0, buffer, j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(currVerts[j].y), 0, buffer, 4 + j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(currVerts[j].z), 0, buffer, 8 + j * 12, 4);
            }
            GetComponent<MeshFilter>().mesh.vertices = currVerts;
            targetMesh = currVerts;
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
            // Buffer.BlockCopy(data, 0, vertices, 0, data.Length);
            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] = new Vector3(BitConverter.ToSingle(data, j * 12), BitConverter.ToSingle(data, j * 12 + 4), BitConverter.ToSingle(data, j * 12 + 8));
            }
            GetComponent<MeshFilter>().mesh.vertices = vertices;
            targetMesh = vertices;
        }

        // add or remove destruction value to ship parts
        public void AddDestruction(float destruc)
        {
            destruction = Mathf.Clamp01(destruction + destruc);
            if (destruc < 0)
                InterpolateCurrentMesh();
        }

        private void InterpolateCurrentMesh()
        {
            Mesh currM = GetComponent<MeshFilter>().mesh;
            Vector3[] vert = currM.vertices;
            for (int i = 0; i < vert.Length; i++)
                vert[i] = Vector3.Lerp(initialMesh[i], targetMesh[i], destruction);
            GetComponent<MeshFilter>().mesh.vertices = vert;
        }
    }
}