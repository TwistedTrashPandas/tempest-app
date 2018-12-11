using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking.Test
{
    public class ExpandSplitNetworkBehaviour : NetworkBehaviour
    {
        private struct MessageExpandSplit
        {
            public bool expand;

            public MessageExpandSplit (bool expand)
            {
                this.expand = expand;
            }
        };

        protected override void UpdateClient()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                MessageExpandSplit message = new MessageExpandSplit(true);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                MessageExpandSplit message = new MessageExpandSplit(false);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
        }

        protected override void OnReceivedMessageRaw(byte[] data, ulong steamID)
        {
            if (serverObject.onServer)
            {
                MessageExpandSplit message = ByteSerializer.FromBytes<MessageExpandSplit>(data);

                if (message.expand)
                {
                    transform.localScale += Vector3.one;
                }
                else
                {
                    // Make smaller and duplicate this object
                    transform.localScale *= 0.5f;
                    Instantiate(gameObject, transform.parent, true);
                }
            }
        }
    }
}
