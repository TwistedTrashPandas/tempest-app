using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MastersOfTempest.Environment
{
    public class EnvironmentBoundaries : NetworkBehaviour
    {
        private const float checkRate = 1f / 16f;

        // setting world boundaries
        private const float damagePerSecond = 0.08f;
        private const float maxHeight = 1250f;
        private const float maxRadius = 3500f;

        // collision with water
        private const float minHeight = 0f;

        // visual feedback parameters for the client
        private const float maxIntensity = 0.5f;
        private const float maxDistance = 200f;


        // shipparts treated separately
        private ShipPart[] shipParts;
        private Vector3 worldCenter;
        private Vignette postProcessVignette;

        // check if position has changed
        private float lastValToSend;

        protected override void StartServer()
        {
            base.StartServer();
            Gamemaster master = GameObject.Find("Gamemaster").GetComponent<Gamemaster>();
            worldCenter = master.GetEnvironmentManager().vectorField.GetCenterWS();
            shipParts = master.GetShip().GetComponentsInChildren<ShipPart>();
            if (shipParts == null)
                throw new System.InvalidOperationException("Ship parts not found.");
            lastValToSend = -1f;
            StartCoroutine(CheckForBoundaries());
        }

        protected override void StartClient()
        {
            base.StartClient();
            postProcessVignette = FindObjectOfType<PostProcessVolume>().profile.GetSetting<Vignette>();
        }

        private IEnumerator CheckForBoundaries()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(checkRate);
                float valToSend = 0f;

                // is any ship part outside the playing area? -> if yes damage it accordingly
                for (int i = 0; i < shipParts.Length; i++)
                {
                    ShipPart curr = shipParts[i];
                    float dist;
                    if (curr.transform.position.y > maxHeight)
                    {
                        curr.AddDestruction(damagePerSecond * Time.fixedDeltaTime);
                        valToSend = curr.transform.position.y - maxHeight;
                    }
                    if ((dist = Vector3.Distance(worldCenter, curr.transform.position)) > maxRadius)
                    {
                        curr.AddDestruction(damagePerSecond * Time.fixedDeltaTime);
                        valToSend = dist - maxRadius;
                    }
                    if (curr.transform.position.y < minHeight)
                    {
                        Debug.Log("set destr water");
                        curr.AddDestruction(1.0f);
                    }
                }

                // send damage update to players for visual feedback
                if (!Mathf.Approximately(lastValToSend, valToSend))
                {
                    SendToAllClients(ByteSerializer.GetBytes(valToSend), Facepunch.Steamworks.Networking.SendType.Reliable);
                    lastValToSend = valToSend;
                }
            }
        }

        // data is just one float for controlling the vignette intensity
        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            float distanceDifference = ByteSerializer.FromBytes<float>(data);
            postProcessVignette.intensity.Override(Mathf.Min(distanceDifference / maxDistance, maxIntensity));
        }
    }
}
