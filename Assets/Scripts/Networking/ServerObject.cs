﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MastersOfTempest.Networking
{
    public class ServerObject : MonoBehaviour
    {
        [Tooltip("Cannot be longer than 36 characters!")]
        public string resourceName = "";

        public bool onServer = true;
        public int serverID = 0;
        public float lastUpdate = 0;

        [Header("Client Parameters")]
        public bool interpolateOnClient = true;
        public bool removeChildColliders = true;
        public bool removeChildRigidbodies = true;

        // Interpolation variables
        private MessageServerObject currentMessage;
        private MessageServerObject lastMessage;
        private float timeSinceLastMessage = 0;
        private uint receivedMessageCount = 0;

        void Start()
        {
            if (onServer)
            {
                // Check if the resource name is valid
                if (Resources.Load<GameObject>("ServerObjects/" + resourceName) == null)
                {
                    Debug.LogError("Cannot find resource \"" + resourceName + "\" of gameobject " + name);
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
                if (receivedMessageCount > 1)
                {
                    // Interpolate between the transform from the last and the current message based on the time that passed since the last message
                    // This introduces a bit of latency but does not require any prediction
                    // Also make sure that the interpolation does not take too long if the time between the messages is long
                    float dt = Mathf.Min(currentMessage.time - lastMessage.time, 1);

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
                    receivedMessageCount++;
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

    [StructLayout(LayoutKind.Sequential)]
    public struct MessageServerObject
    {
        public float time;                                          // 4 bytes
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string name;                                         // 24 bytes
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        public string resourceName;                                 // 36 bytes
        public bool hasParent;                                      // 4 byte
        public int parentInstanceID;                                // 4 bytes
        public int instanceID;                                      // 4 bytes
        public Vector3 localPosition;                               // 12 bytes
        public Quaternion localRotation;                            // 16 bytes
        public Vector3 localScale;                                  // 12 bytes
                                                                    // 116 bytes

        public MessageServerObject(ServerObject serverObject)
        {
            if (serverObject.transform.parent != null)
            {
                parentInstanceID = serverObject.transform.parent.GetInstanceID();
                hasParent = true;
            }
            else
            {
                parentInstanceID = 0;
                hasParent = false;
            }

            time = serverObject.lastUpdate = Time.time;
            name = serverObject.name;
            resourceName = serverObject.resourceName;
            instanceID = serverObject.transform.GetInstanceID();
            localPosition = serverObject.transform.localPosition;
            localRotation = serverObject.transform.localRotation;
            localScale = serverObject.transform.localScale;

            if (serverObject.resourceName.Length > 36)
            {
                Debug.LogError("Resource name on " + serverObject.name + " is but cannot be longer than 36 characters!");
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MessageServerObjectList
    {
        public int count;                                           // 4 bytes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public MessageServerObject[] messages;                      // 1160 bytes
                                                                    // 1164 bytes
    }
}
