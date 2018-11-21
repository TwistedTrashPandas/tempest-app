﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use this class for custom networking messages between the client and the server
// Requires a ServerObject to be attached to this gameObject
// The transform is automatically synchronized already by the ServerObject

// NOTES:
// - Rename this class and the message struct
// - Make sure that the public member networkMessageType is set to the type of your message (e.g. the name of this class)
// - Only use SendToServer(...) as a client and let the server answer with SendToClient(...) or SendToAllClients(...)
// - If you have to use the MonoBehaviour Start(), Update() and OnDestroy() methods you have to call base.Start() / base.Update() / base.OnDestroy() inside
// - If you want to synchronize a lot of objects it is more performant to only have one NetworkBehaviour that handles e.g. a list of objects that you want to synchronize
public class NetworkBehaviourTemplate : NetworkBehaviour
{
    // Example: synchronize this variable which is randomly set by the server
    public int myVariable = 0;

    [System.Serializable]
    private struct NetworkBehaviourTemplateMessage
    {
        // Declare all your variables that you want to send over the network here

        // Example: store myVariable from the server in this message
        public int serverMyVariable;
    };

    // Used for initialization when this object is on a client
    protected override void StartClient()
    {
        // Example: ask the server for the value of the variable
        SendToServer("Hello server, can you please send me the value of your variable?");
    }

    // Used for initialization when this object is on the server
    protected override void StartServer()
    {
        // Example: set random value for myVariable
        myVariable = Random.Range(0, 100);
    }

    // Called each frame when this object is on a client
    protected override void UpdateClient()
    {
    }

    // Called each frame when this object is on the server
    protected override void UpdateServer()
    {
    }

    // Called when the object is on a client and receives a message
    protected override void OnClientReceivedMessage(string message, ulong steamID)
    {
        // Example: deserialize the message in order to read the variable value
        NetworkBehaviourTemplateMessage tmp = JsonUtility.FromJson<NetworkBehaviourTemplateMessage>(message);

        // Example: update the value of myVariable
        myVariable = tmp.serverMyVariable;
    }

    // Called when the object is on the server and receives a message
    // Use SendToClient(...) or SendToAllClients(...) for answeing / broadcasting
    protected override void OnServerReceivedMessage(string message, ulong steamID)
    {
        // Example: create a new NetworkBehaviourTemplateMessage
        NetworkBehaviourTemplateMessage tmp;
        tmp.serverMyVariable = myVariable;

        // Example: broadcasting the message to all clients
        SendToAllClients(JsonUtility.ToJson(tmp));
    }

    // Called when this object is destroyed on a client
    // Do not send any messages to the server here (the receiving object on the server is probably already destroyed)
    protected override void OnDestroyClient()
    {
    }

    // Called when this object is destroyed on the server
    // Do not send any messages to the client here (the receiving object on the client will be destroyed soon)
    protected override void OnDestroyServer()
    {
    }
}
