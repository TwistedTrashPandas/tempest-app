using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;

namespace MastersOfTempest.PlayerControls
{
    public class TranformManipulator : NetworkBehaviour
    {
        [Serializable]
        private struct TransformMessage
        {
            public bool changePosition;
            public float x, y, z;
            public bool changeRotation;
            public float rx, ry, rz;

            public Vector3 Position
            {
                get
                {
                    return new Vector3(x, y, z);
                }
            }
            public Vector3 Rotation
            {
                get
                {
                    return new Vector3(rx, ry, rz);
                }
            }
        }

        public void ChangeTransform(Vector3 position, Vector3 rotation)
        {
            if (serverObject.onServer)
            {
                this.transform.position = position;
                this.transform.localEulerAngles = rotation;
            }
            else
            {
                var message = new TransformMessage
                {
                    changePosition = true,
                    x = position.x,
                    y = position.y,
                    z = position.z,
                    changeRotation = true,
                    rx = rotation.x,
                    ry = rotation.y,
                    rz = rotation.z
                };
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        public void ChangePosition(Vector3 position)
        {
            if (serverObject.onServer)
            {
                this.transform.position = position;
            }
            else
            {
                var message = new TransformMessage
                {
                    changePosition = true,
                    x = position.x,
                    y = position.y,
                    z = position.z,
                    changeRotation = false
                };
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        public void ChangeRotation(Vector3 rotation)
        {
            if (serverObject.onServer)
            {
                this.transform.localEulerAngles = rotation;
            }
            else
            {
                var message = new TransformMessage
                {
                    changePosition = false,
                    changeRotation = true,
                    rx = rotation.x,
                    ry = rotation.y,
                    rz = rotation.z
                };
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var message = ByteSerializer.FromBytes<TransformMessage>(data);
            if (message.changePosition && message.changeRotation)
            {
                ChangeTransform(message.Position, message.Rotation);
            }
            else if(message.changePosition)
            {
                ChangePosition(message.Position);
            }
            else 
            {
                ChangeRotation(message.Rotation);
            }
        }

    }
}
