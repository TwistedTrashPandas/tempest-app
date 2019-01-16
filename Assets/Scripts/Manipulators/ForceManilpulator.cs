using System;
using System.Collections;
using MastersOfTempest.Networking;
using UnityEngine;

namespace MastersOfTempest.ShipBL
{
    public class ForceManilpulator : NetworkBehaviour
    {
        private new Rigidbody rigidbody;

        [Serializable]
        private struct ForceManipulatorNetworkMessage
        {
            //Unity's Vector3 is not serializable :(
            public Vector3 Force { get { return new Vector3(x, y, z); } }

            public float x;
            public float y;
            public float z;
            public float duration;

            public ForceManipulatorNetworkMessage(Vector3 force, float duration)
            {
                x = force.x;
                y = force.y;
                z = force.z;
                this.duration = duration;
            }
        }

        protected override void StartServer()
        {
            base.StartServer();
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                throw new InvalidOperationException($"{nameof(rigidbody)} is not specified!");
            }
        }

        protected override void OnServerReceivedMessage(string message, ulong steamID)
        {
            base.OnServerReceivedMessage(message, steamID);
            var msg = JsonUtility.FromJson<ForceManipulatorNetworkMessage>(message);
            AddForce(msg.Force, msg.duration);
        }

        public void AddForce(Vector3 force)
        {
            AddForce(force, 0f);
        }

        public void AddForce(Vector3 force, float duration)
        {
            //we apply specified force when on the server
            if (serverObject.onServer)
            {
                rigidbody.AddForce(force);
            }
            //we send force to be applied from the client
            else
            {
                var msg = JsonUtility.ToJson(new ForceManipulatorNetworkMessage(force, duration));
                SendToServer(msg);
            }
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position)
        {
            //TODO: actually use the position
            AddForce(force);
        }
        public void AddForceAtPosition(Vector3 force, Vector3 position, float duration)
        {
            //TODO: actually use the position
            AddForce(force, duration);
        }

        private IEnumerator RemoveForce(Vector3 force, float time)
        {
            yield return new WaitForSeconds(time);
            rigidbody.AddForce(-force);
        }
    }
}
