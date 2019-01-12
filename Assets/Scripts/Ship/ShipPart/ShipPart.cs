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
        public ShipPartArea interactionArea;
        public float cutOffDist = 25f;
        public float impulseScaling = 0.15f;
        public float maxDisplacementDist = 20f;
        /// <summary>
        /// destruction == 0:   ship part fully repaired
        ///             == 1:   ship part fully destroyed
        /// </summary>
        public float destruction;
        public ShipPartStatus status;

        private Vector3[] initialMesh;
        private Vector3[] targetMesh;
        private Material material;

        protected override void Start()
        {
            base.Start();
            initialMesh = GetComponent<MeshFilter>().mesh.vertices;
            targetMesh = initialMesh;
            if (initialMesh == null)
                throw new System.InvalidOperationException("Ship part can only be attached to objects with meshes");
            material = GetComponent<MeshRenderer>().material;
        }

        public float GetDestruction()
        {
            return destruction;
        }

        // updates mesh depending on collision (usually called from damaging objects such as rocks)
        public void ResolveCollision(float destruc, ContactPoint[] contactPoints, Vector3 impulse)
        {
            AddDestruction(destruc);
            SendCollision(contactPoints, impulse, destruc);
        }

        private void SendCollision(ContactPoint[] contactPoints, Vector3 impulse, float destruc)
        {
            byte[] buffer = new byte[16 + contactPoints.Length * 12];

            Buffer.BlockCopy(BitConverter.GetBytes(destruc), 0, buffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(impulse.x), 0, buffer, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(impulse.y), 0, buffer, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(impulse.z), 0, buffer, 12, 4);
            for (int j = 0; j < contactPoints.Length; j++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(contactPoints[j].point.x), 0, buffer, 16 + j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(contactPoints[j].point.y), 0, buffer, 20 + j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(contactPoints[j].point.z), 0, buffer, 24 + j * 12, 4);
            }
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        public void UpdateMesh(Vector3[] contactPoints, Vector3 impulse)
        {
            Vector3[] currVerts = GetComponent<MeshFilter>().mesh.vertices;

            for (int i = 0; i < contactPoints.Length; i++)
            {
                Vector3 currContact = contactPoints[i];
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

                        Vector3 initialPos = transform.TransformPoint(initialMesh[j]);
                        dir = initialPos - worldPos;
                        if (dir.magnitude > maxDisplacementDist)
                            worldPos = initialPos + dir.normalized * maxDisplacementDist;
                    }
                    currVerts[j] = transform.InverseTransformPoint(worldPos);
                }
            }
            targetMesh = currVerts;
            InterpolateCurrentMesh();
            /*            
            byte[] buffer = new byte[currVerts.Length * 12];
            for (int j = 0; j < currVerts.Length; j++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(currVerts[j].x), 0, buffer, j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(currVerts[j].y), 0, buffer, 4 + j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(currVerts[j].z), 0, buffer, 8 + j * 12, 4);
            }
            GetComponent<MeshFilter>().mesh.vertices = currVerts;
            targetMesh = currVerts;
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);*/
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnServerReceivedMessageRaw(data, steamID);
            HandleDataReceive(data);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            HandleDataReceive(data);
        }

        private void HandleDataReceive(byte[] data)
        {
            /*
            Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
            // Buffer.BlockCopy(data, 0, vertices, 0, data.Length);
            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] = new Vector3(BitConverter.ToSingle(data, j * 12), BitConverter.ToSingle(data, j * 12 + 4), BitConverter.ToSingle(data, j * 12 + 8));
            }
            GetComponent<MeshFilter>().mesh.vertices = vertices;
            targetMesh = vertices;*/
            int l = Mathf.FloorToInt(data.Length / 12) - 1;

            SetDestruction(BitConverter.ToSingle(data, 0));
            Vector3 impulse = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
            Vector3[] contactPoints = new Vector3[l];
            for (int j = 0; j < l; j++)
            {
                contactPoints[j] = new Vector3(BitConverter.ToSingle(data, j * 12 + 16), BitConverter.ToSingle(data, j * 12 + 20), BitConverter.ToSingle(data, j * 12 + 24));
            }
            UpdateMesh(contactPoints, impulse);
        }

        // add or remove destruction value to ship parts
        public void AddDestruction(float destruc)
        {
            if (status == ShipPartStatus.Fragile)
                destruction = 1.0f;
            else
                destruction = Mathf.Clamp01(destruction + destruc);
        }

        // add or remove destruction value to ship parts
        public void SetDestruction(float destruc)
        {
            destruction = destruc;
        }

        // interpolate between damaged mesh and initial mesh
        private void InterpolateCurrentMesh()
        {
            Mesh currM = GetComponent<MeshFilter>().mesh;
            Vector3[] vert = currM.vertices;
            for (int i = 0; i < vert.Length; i++)
                vert[i] = Vector3.Lerp(initialMesh[i], targetMesh[i], destruction);
            GetComponent<MeshFilter>().mesh.vertices = vert;
            
            /*
            byte[] buffer = new byte[vert.Length * 12];
            for (int j = 0; j < vert.Length; j++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(vert[j].x), 0, buffer, j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(vert[j].y), 0, buffer, 4 + j * 12, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(vert[j].z), 0, buffer, 8 + j * 12, 4);
            }
            GetComponent<MeshFilter>().mesh.vertices = vert;
            targetMesh = vert;
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);*/
        }

        public void ChangeShaderDestructionValue()
        {
            material.SetFloat("_fDestruction", destruction);
        }

        public void ResetShaderDestructionValue()
        {
            material.SetFloat("_fDestruction", 0.0f);
        }
    }
}