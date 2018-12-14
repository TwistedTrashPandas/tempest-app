using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    public class MessageNetworkBehaviour
    {
        // Fixed size
        public int serverID;                                        // 4 bytes
        public int typeID;                                          // 4 bytes

        // Dynamic size
        public byte[] data;                                         // ? bytes

        public MessageNetworkBehaviour ()
        {
            // Empty constructor
        }

        public MessageNetworkBehaviour(int serverID, int typeID, byte[] data)
        {
            this.serverID = serverID;
            this.typeID = typeID;
            this.data = data;
        }

        public byte[] ToBytes()
        {
            ArrayList bytes = new ArrayList();

            // Fixed size
            bytes.AddRange(BitConverter.GetBytes(serverID));
            bytes.AddRange(BitConverter.GetBytes(typeID));

            // Dynamic size
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);

            return (byte[])bytes.ToArray(typeof(byte));
        }

        public static MessageNetworkBehaviour FromBytes(byte[] data, int startIndex)
        {
            MessageNetworkBehaviour messageNetworkBehaviour = new MessageNetworkBehaviour();

            // Fixed size
            messageNetworkBehaviour.serverID = BitConverter.ToInt32(data, startIndex);
            messageNetworkBehaviour.typeID = BitConverter.ToInt32(data, startIndex + 4);

            // Dynamic size
            int dataLength = BitConverter.ToInt32(data, startIndex + 8);
            messageNetworkBehaviour.data = new byte[dataLength];
            Array.Copy(data, startIndex + 12, messageNetworkBehaviour.data, 0, dataLength);

            return messageNetworkBehaviour;
        }
    };
}
