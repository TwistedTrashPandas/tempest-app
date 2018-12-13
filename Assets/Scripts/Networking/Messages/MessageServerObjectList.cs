using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MastersOfTempest.Networking
{
    class MessageServerObjectList
    {
        // Dynamic size
        public LinkedList<MessageServerObject> messages = new LinkedList<MessageServerObject>();    // ? bytes

        public byte[] GetBytes()
        {
            ArrayList data = new ArrayList();

            // Dynamic size
            data.AddRange(BitConverter.GetBytes(messages.Count));

            foreach (MessageServerObject m in messages)
            {
                byte[] mData = m.GetBytes();
                data.AddRange(BitConverter.GetBytes(mData.Length));
                data.AddRange(mData);
            }

            return (byte[])data.ToArray(typeof(byte));
        }

        public static MessageServerObjectList FromBytes(byte[] data, int startIndex)
        {
            MessageServerObjectList messageServerObjectList = new MessageServerObjectList();

            // Get messages length
            int messagesCount = BitConverter.ToInt32(data, startIndex);
            int index = startIndex + 4;

            // Read and assign all the dynamically sized server object messages
            for (int i = 0; i < messagesCount; i++)
            {
                int mLength = BitConverter.ToInt32(data, index);
                index += 4;

                MessageServerObject m = MessageServerObject.FromBytes(data, index);
                index += mLength;

                messageServerObjectList.messages.AddLast(m);
            }

            return messageServerObjectList;
        }
    }
}