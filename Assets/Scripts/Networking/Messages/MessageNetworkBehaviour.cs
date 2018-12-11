using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MastersOfTempest.Networking
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MessageNetworkBehaviour
    {
        public int serverID;                                        // 4 bytes
        public int typeID;                                          // 4 bytes
        public int dataLength;                                      // 4 bytes
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1188)]
        public byte[] data;                                         // 1188 bytes
                                                                    // 1200 bytes

        public MessageNetworkBehaviour(int serverID, int typeID, byte[] data)
        {
            this.serverID = serverID;
            this.typeID = typeID;

            // TODO: check if a variable size works (saves data)
            this.data = new byte[1188];
            System.Array.Copy(data, this.data, data.Length);
            this.dataLength = data.Length;
        }
    };
}
