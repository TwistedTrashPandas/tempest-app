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
        [ReadOnly]
        public bool onServer = true;
        [ReadOnly]
        public int serverID = 0;
        [ReadOnly]
        public float lastUpdate = 0;
        
        // Used when the server object is a child in a server object resource (resourceID < 0), set by editor script
        [SerializeField, HideInInspector]
        public ServerObject root = null;
        [SerializeField, HideInInspector]
        public ServerObject[] children = null;

        [Header("Server Parameters")]
        public string serverLayer = "Server";
        public bool removeServerChildColliders = false;

        [Header("Client Parameters")]
        public bool interpolateOnClient = true;
        public bool removeChildColliders = true;
        public bool removeChildRigidbodies = true;
        public bool setChildCollidersTriggers = true;

        // Interpolation variables
        private MessageServerObject currentMessage = null;
        private MessageServerObject lastMessage = null;
        private float timeSinceLastMessage = 0;

        // Handles all the incoming network behaviour messages from the network behaviours
        private Dictionary<int, Action<byte[], ulong>> networkBehaviourEvents = new Dictionary<int, Action<byte[], ulong>>();
        private Dictionary<int, Action<ulong>> networkBehaviourInitializedEvents = new Dictionary<int, Action<ulong>>();

        private Vector3 lastLocalPosition;
        private Quaternion lastLocalRotation;
        private Vector3 lastLocalScale;

        void Start()
        {
            if (onServer)
            {
                // Check if the resource id is valid
                if (resourceID == 0)
                {
                    Debug.LogError("Resource id is not valid for ServerObject " + gameObject.name);
                }

                // Set server ID
                serverID = transform.GetInstanceID();

                // Set layer, also for children
                SetLayerOfThisGameObjectAndAllChildren(serverLayer);

                // Remove colliders on the server
                RemoveCollidersAndRigidbodiesServer();

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

        private void RemoveCollidersAndRigidbodies()
        {
            if (removeChildColliders)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();

                foreach (Collider c in colliders)
                {
                    Destroy(c);
                }
            }

            if (setChildCollidersTriggers)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();

                foreach (Collider c in colliders)
                {
                    if (!(c is CharacterController))
                        c.isTrigger = true;
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

        private void RemoveCollidersAndRigidbodiesServer()
        {
            if (removeServerChildColliders)
            {
                Collider[] colliders = GetComponentsInChildren<Collider>();

                foreach (Collider c in colliders)
                {
                    Destroy(c);
                }
            }
        }

        public bool HasChanged ()
        {
            bool changed = (transform.localPosition != lastLocalPosition) | (transform.localRotation != lastLocalRotation) | (transform.localScale != lastLocalScale);

            // Save new values
            lastLocalPosition = transform.localPosition;
            lastLocalRotation = transform.localRotation;
            lastLocalScale = transform.localScale;

            return changed;
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

        public void HandleNetworkBehaviourInitializedMessage (int index, ulong steamID)
        {
            networkBehaviourInitializedEvents[index].Invoke(steamID);
        }

        public void HandleNetworkBehaviourMessage(int index, byte[] data, ulong steamId)
        {
            networkBehaviourEvents[index].Invoke(data, steamId);
        }

        public void AddNetworkBehaviourEvents(int index, Action<byte[], ulong> behaviourAction, Action<ulong> initializedAction)
        {
            networkBehaviourEvents[index] = behaviourAction;
            networkBehaviourInitializedEvents[index] = initializedAction;
        }

        public void RemoveNetworkBehaviourEvents(int index)
        {
            networkBehaviourEvents.Remove(index);
            networkBehaviourInitializedEvents.Remove(index);
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
