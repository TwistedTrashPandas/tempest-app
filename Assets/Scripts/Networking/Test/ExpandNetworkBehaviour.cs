using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking.Test
{
    public class ExpandNetworkBehaviour : NetworkBehaviour
    {
        private struct CubeOtherNetworkMessage
        {
            public bool expand;

            public CubeOtherNetworkMessage (bool expand)
            {
                this.expand = expand;
            }
        };

        protected override void StartClient()
        {

        }

        protected override void UpdateClient()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CubeOtherNetworkMessage message = new CubeOtherNetworkMessage(true);
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
            }
        }
    }
}
