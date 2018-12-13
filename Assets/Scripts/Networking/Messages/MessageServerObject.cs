using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MastersOfTempest.Networking
{
    // This struct has a fixed size and can therefore be nicely packed into UDP/TCP messages
    public struct MessageServerObject
    {
        public float time;                                          // 4 bytes
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string name;                                         // 24 bytes
        public int resourceID;                                      // 4 bytes
        public bool hasParent;                                      // 4 byte
        public int parentInstanceID;                                // 4 bytes
        public int instanceID;                                      // 4 bytes
        public float localPositionX;                                // 4 bytes
        public float localPositionY;                                // 4 bytes
        public float localPositionZ;                                // 4 bytes
        public float localRotationX;                                // 4 bytes
        public float localRotationY;                                // 4 bytes
        public float localRotationZ;                                // 4 bytes
        public float localRotationW;                                // 4 bytes
        public float localScaleX;                                   // 4 bytes
        public float localScaleY;                                   // 4 bytes
        public float localScaleZ;                                   // 4 bytes
                                                                    // 84 bytes

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

            // Set and save the last update time of the server object
            time = serverObject.lastUpdate = Time.time;
            name = serverObject.name;
            resourceID = serverObject.resourceID;
            instanceID = serverObject.transform.GetInstanceID();

            // Position
            localPositionX = serverObject.transform.localPosition.x;
            localPositionY = serverObject.transform.localPosition.y;
            localPositionZ = serverObject.transform.localPosition.z;

            // Rotation
            localRotationX = serverObject.transform.localRotation.x;
            localRotationY = serverObject.transform.localRotation.y;
            localRotationZ = serverObject.transform.localRotation.z;
            localRotationW = serverObject.transform.localRotation.w;

            // Scale
            localScaleX = serverObject.transform.localScale.x;
            localScaleY = serverObject.transform.localScale.y;
            localScaleZ = serverObject.transform.localScale.z;
        }

        public Vector3 GetLocalPosition ()
        {
            return new Vector3(localPositionX, localPositionY, localPositionZ);
        }

        public Quaternion GetLocalRotation()
        {
            return new Quaternion(localRotationX, localRotationY, localRotationZ, localRotationW);
        }

        public Vector3 GetLocalScale()
        {
            return new Vector3(localScaleX, localScaleY, localScaleZ);
        }
    }
}
