using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MastersOfTempest.Networking
{
    // We could not send the name to save data for better performance
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageServerObject
    {
        public float time;                                          // 4 bytes
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
        public string name;                                         // 24 bytes
        public int resourceID;                                      // 4 bytes
        public bool hasParent;                                      // 4 byte
        public int parentInstanceID;                                // 4 bytes
        public int instanceID;                                      // 4 bytes
        public Vector3 localPosition;                               // 12 bytes
        public Quaternion localRotation;                            // 16 bytes
        public Vector3 localScale;                                  // 12 bytes
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

            time = serverObject.lastUpdate = Time.time;
            name = serverObject.name;
            resourceID = serverObject.resourceID;
            instanceID = serverObject.transform.GetInstanceID();
            localPosition = serverObject.transform.localPosition;
            localRotation = serverObject.transform.localRotation;
            localScale = serverObject.transform.localScale;
        }
    }
}
