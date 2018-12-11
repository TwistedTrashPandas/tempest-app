using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking.Test
{
    public class ExpandSplitNetworkBehaviour : NetworkBehaviour
    {
        private struct CubeOtherNetworkMessage
        {
            public bool expand;

            public CubeOtherNetworkMessage (bool expand)
            {
                this.expand = expand;
            }
        };

        protected override void UpdateClient()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CubeOtherNetworkMessage message = new CubeOtherNetworkMessage(true);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                CubeOtherNetworkMessage message = new CubeOtherNetworkMessage(false);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
        }

        protected override void OnReceivedMessageRaw(byte[] data, ulong steamID)
        {
            if (serverObject.onServer)
            {
                CubeOtherNetworkMessage cubeOtherNetworkMessage = ByteSerializer.FromBytes<CubeOtherNetworkMessage>(data);

                if (cubeOtherNetworkMessage.expand)
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
