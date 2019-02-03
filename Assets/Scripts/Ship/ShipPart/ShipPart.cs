using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using MastersOfTempest.Networking;
using System;
using System.Linq;

namespace MastersOfTempest.ShipBL
{
    public class ShipPart : NetworkBehaviour
    {
        public event EventHandler ShipPartHit;
        public ShipPartArea interactionArea;
        private const float cutOffDist = 10.0f;
        private const float impulseScaling = 0.66f;
        private const float maxDisplacementDist = 0.5f;
        /// <summary>
        /// destruction == 0:   ship part fully repaired
        ///             == 1:   ship part fully destroyed
        /// </summary>
        public float destruction;
        public ShipPartStatus status;
        public AudioClip crashSound;
        public ShipPart nextAreaPart;

        private Vector3[] initialMesh;
        private Vector3[] targetMesh;
        private Material material;
        private LoseCondition loseCondition;
        private AudioSource audioSource;

        protected override void Start()
        {
            base.Start();
            initialMesh = GetComponent<MeshFilter>().mesh.vertices;
            targetMesh = initialMesh;
            if (initialMesh == null)
                throw new System.InvalidOperationException("Ship part can only be attached to objects with meshes");
            material = GetComponent<MeshRenderer>().material;
            loseCondition = FindObjectsOfType<Gamemaster>().First(gm => gm.gameObject.scene == gameObject.scene).GetComponent<LoseCondition>();
        }

        protected override void StartClient()
        {
            base.StartClient();
            audioSource = GetComponent<AudioSource>();
        }


        protected override void StartServer()
        {
            base.StartClient();
            Destroy(GetComponent<AudioSource>());
        }

        protected override void UpdateServer()
        {
            base.UpdateServer();

        }

        public float GetDestruction()
        {
            return destruction;
        }

        // updates mesh depending on collision (usually called from damaging objects such as rocks)
        public void ResolveCollision(float destruc, ContactPoint[] contactPoints, Vector3 impulse)
        {
            // transfer damage to next shippart
            if (Mathf.Approximately(destruction, 1.0f) && destruc > 0.05f)
                nextAreaPart.ResolveCollision((destruc - 0.5f), contactPoints, impulse / 2f);
            else
            {
                if ((status & ShipPartStatus.Fragile) == ShipPartStatus.Fragile)
                {
                    destruc = 1.0f;
                }
                ShipPartHit?.Invoke(this, new ShipPartHitEventArgs(destruc));
                SendCollision(contactPoints, impulse, destruc);
                AddDestruction(destruc);

                // transfer damage to next ship part
                if (destruc > 1.05f)
                    nextAreaPart.ResolveCollision((destruc - 0.5f) / 1.5f, contactPoints, impulse / 2f);
            }
        }

        private void SendCollision(ContactPoint[] contactPoints, Vector3 impulse, float destruc)
        {
            byte[] buffer = new byte[16 + contactPoints.Length * 12];

            Buffer.BlockCopy(BitConverter.GetBytes(destruc), 0, buffer, 0, 4);  // 4 bytes destruc value
            Buffer.BlockCopy(BitConverter.GetBytes(impulse.x), 0, buffer, 4, 4);// 12 bytes impulse value
            Buffer.BlockCopy(BitConverter.GetBytes(impulse.y), 0, buffer, 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(impulse.z), 0, buffer, 12, 4);
            // n * 12 bytes for the generated contactPoints
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
                        Vector3 dir = (worldPos - currContact).normalized; // impulse; // 
                        dir *= Mathf.Min(impulse.magnitude, 1000.0f);
                        distSquared *= distSquared;
                        worldPos += dir / (distSquared + 1f) / contactPoints.Length * impulseScaling;
                        Vector3 initialPos = transform.TransformPoint(initialMesh[j]);
                        dir = initialPos - worldPos;
                        if (dir.sqrMagnitude > maxDisplacementDist * maxDisplacementDist)
                            worldPos = initialPos + dir.normalized * maxDisplacementDist;
                    }
                    currVerts[j] = transform.InverseTransformPoint(worldPos);
                }
            }
            targetMesh = currVerts;
            InterpolateCurrentMesh();
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
            if (data.Length == 4)
            {
                SetDestruction(BitConverter.ToSingle(data, 0));
            }
            else
            {
                float destruc = BitConverter.ToSingle(data, 0);

                // length of contact off set array
                int l = Mathf.FloorToInt(data.Length / 12) - 1;

                // first 4 bytes are the delta destruction value
                if (serverObject.onServer)
                    AddDestruction(destruc);
                //else
                //    SetDestruction(destruc);


                // crash sound, played locally at ship part
                audioSource.PlayOneShot(crashSound, Mathf.Clamp(Mathf.Clamp01(destruc) / 2.0f, 0.15f, 0.45f));

                // next 12 bytes are the values for the impulse vector
                Vector3 impulse = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));

                // other bytes are the contact point vectors
                Vector3[] contactPoints = new Vector3[l];
                for (int j = 0; j < l; j++)
                {
                    contactPoints[j] = new Vector3(BitConverter.ToSingle(data, j * 12 + 16), BitConverter.ToSingle(data, j * 12 + 20), BitConverter.ToSingle(data, j * 12 + 24));
                }
                UpdateMesh(contactPoints, impulse);
            }
        }

        // add or remove destruction value to ship parts
        public void AddDestruction(float destruc)
        {
            destruction = Mathf.Clamp01(destruction + destruc);

            // lose condition checks if the overall destruction value is above the threshold (after collision)
            if (destruc > 0f)
                loseCondition.CheckOverAllDestruction();

            // sends updated destruction value separately
            byte[] buffer = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes(destruction), 0, buffer, 0, 4);
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        // add or remove destruction value to ship parts
        public void SetDestruction(float destruc)
        {
            destruction = destruc;
            InterpolateCurrentMesh();
        }

        // interpolate between damaged mesh and initial mesh
        private void InterpolateCurrentMesh()
        {
            Mesh currM = GetComponent<MeshFilter>().mesh;
            Vector3[] vert = currM.vertices;
            for (int i = 0; i < vert.Length; i++)
                vert[i] = Vector3.Lerp(initialMesh[i], targetMesh[i], destruction);
            GetComponent<MeshFilter>().mesh.vertices = vert;
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
