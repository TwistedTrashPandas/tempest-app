using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeleportArea))]
public class TeleportActionNetworked : NetworkBehaviour
{
    private struct TeleportMessage
    {
        public int objectToTeleportServerID;
        public bool goBack;
    }

    public void TeleportOnServer (int objectToTeleportServerID, bool goBack)
    {
        if (serverObject.onServer)
        {
            if (goBack)
            {
                // Go to the return transform
                GameServer.Instance.GetServerObject(objectToTeleportServerID).transform.position = GetComponent<TeleportArea>().returnTransform.position;
            }
            else
            {
                // Go to the teleport area
                GameServer.Instance.GetServerObject(objectToTeleportServerID).transform.position = transform.position;
            }
        }
        else
        {
            TeleportMessage message = new TeleportMessage();
            message.objectToTeleportServerID = objectToTeleportServerID;
            message.goBack = goBack;
            SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
        }
    }

    protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
    {
        TeleportMessage message = ByteSerializer.FromBytes<TeleportMessage>(data);
        TeleportOnServer(message.objectToTeleportServerID, message.goBack);
    }
}
