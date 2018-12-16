﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    [DisallowMultipleComponent]
    public class ServerObject : MonoBehaviour
    {
        [ReadOnly]
        public int resourceID = -1;
        public bool onServer = true;
        public int serverID = 0;
        public float lastUpdate = 0;

        [Header("Client Parameters")]
        public bool interpolateOnClient = true;
        public bool removeChildColliders = true;
        public bool removeChildRigidbodies = true;

        // Interpolation variables
        private MessageServerObject currentMessage = null;
        private MessageServerObject lastMessage = null;
        private float timeSinceLastMessage = 0;

        // Handles all the incoming network behaviour messages from the network behaviours
        private Dictionary<int, Action<byte[], ulong>> networkBehaviourEvents = new Dictionary<int, Action<byte[], ulong>>();
        private Dictionary<int, Action<ulong>> networkBehaviourInitializedEvents = new Dictionary<int, Action<ulong>>();

        void Start()
        {
            if (onServer)
            {
                // Check if the resource id is valid
                if (resourceID < 0)
                {
                    Debug.LogError("Resource id " + resourceID + " is not valid for ServerObject " + gameObject.name);
                }

                // Set server ID
                serverID = transform.GetInstanceID();

                // Set layer, also for children
                SetLayerOfThisGameObjectAndAllChildren("Server");

                // Register to game server
                GameServer.Instance.RegisterAndSendMessageServerObject(this);
            }
            else
            {
                SetLayerOfThisGameObjectAndAllChildren("Client");

                // Remove collider / rigidbody on the client because it is not needed most of the time
                RemoveCollidersAndRigidbodies();
            }
        }

        void Update()
        {
            if (!onServer && interpolateOnClient)
            {
                // Make sure that both messages exist
                if (currentMessage != null && lastMessage != null)
                {
                    // Interpolate between the transform from the last and the current message based on the time that passed since the last message
                    // This introduces a bit of latency but does not require any prediction
                    float dt = currentMessage.time - lastMessage.time;

                    if (dt > 0)
                    {
                        float interpolationFactor = timeSinceLastMessage / dt;
                        transform.localPosition = Vector3.Lerp(lastMessage.localPosition, currentMessage.localPosition, interpolationFactor);
                        transform.localRotation = Quaternion.Lerp(lastMessage.localRotation, currentMessage.localRotation, interpolationFactor);
                        transform.localScale = Vector3.Lerp(lastMessage.localScale, currentMessage.localScale, interpolationFactor);
                        timeSinceLastMessage += Time.deltaTime;
                    }
                }
            }
        }

        private void RemoveCollidersAndRigidbodies ()
        {
            if (removeChildColliders)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();

                foreach (Collider c in colliders)
                {
                    Destroy(c);
                }
            }

            if (removeChildRigidbodies)
            {
                Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

                foreach (Rigidbody r in rigidbodies)
                {
                    Destroy(r);
                }
            }
        }

        public void UpdateTransformFromMessageServerObject(MessageServerObject messageServerObject)
        {
            if (!onServer)
            {
                if (interpolateOnClient)
                {
                    // Save data for interpolation
                    lastMessage = currentMessage;
                    currentMessage = messageServerObject;
                    timeSinceLastMessage = 0;

                    // Correct the time of the last message to the time where it should have arrived based on the server hz
                    // This improves the interpolation when the actual time between messages is way larger than the server hz
                    if (lastMessage != null)
                    {
                        lastMessage.time = currentMessage.time - (1.0f / GameClient.Instance.GetServerHz());
                    }
                }
                else
                {
                    // Directly update the transform
                    transform.localPosition = messageServerObject.localPosition;
                    transform.localRotation = messageServerObject.localRotation;
                    transform.localScale = messageServerObject.localScale;
                }
            }
        }

        public void HandleNetworkBehaviourInitializedMessage (int networkBehaviourTypeId, ulong steamID)
        {
            networkBehaviourInitializedEvents[networkBehaviourTypeId].Invoke(steamID);
        }

        public void HandleNetworkBehaviourMessage(int networkBehaviourTypeId, byte[] data, ulong steamId)
        {
            networkBehaviourEvents[networkBehaviourTypeId].Invoke(data, steamId);
        }

        public void AddNetworkBehaviourEvents(int networkBehaviourTypeId, Action<byte[], ulong> behaviourAction, Action<ulong> initializedAction)
        {
            networkBehaviourEvents[networkBehaviourTypeId] = behaviourAction;
            networkBehaviourInitializedEvents[networkBehaviourTypeId] = initializedAction;
        }

        public void RemoveNetworkBehaviourEvents(int networkBehaviourTypeId)
        {
            networkBehaviourEvents.Remove(networkBehaviourTypeId);
            networkBehaviourInitializedEvents.Remove(networkBehaviourTypeId);
        }

        public void SetLayerOfThisGameObjectAndAllChildren (string layer)
        {
            int layerToSet = LayerMask.NameToLayer(layer);

            Transform[] children = GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                child.gameObject.layer = layerToSet;
            }

            gameObject.layer = layerToSet;
        }

        void OnDestroy()
        {
            if (onServer)
            {
                // Send destroy message
                GameServer.Instance.RemoveServerObject(this);
                GameServer.Instance.SendMessageDestroyServerObject(this);
            }
        }
    }
}
