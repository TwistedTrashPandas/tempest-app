using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

namespace MastersOfTempest.PlayerControls
{
    public class CharacterPositionManipulator : NetworkBehaviour
    {
        public CharacterController Character;

        private Rigidbody shipPhysics;

        public Rigidbody ShipPhysics
        {
            get
            {
                if (serverObject.onServer && shipPhysics == null)
                {
                    shipPhysics = FindObjectsOfType<ShipBL.Ship>().First(ship => ship.gameObject.scene == gameObject.scene).GetComponent<Rigidbody>();
                }
                return shipPhysics;
            }
        }
        private const float speed = .6f;

        [Serializable]
        private struct MoveMessage
        {
            public MoveMessage(float horizontal, float vertical, Vector3 forward, Vector3 right, uint msgNumber)
            {
                this.horizontal = horizontal;
                this.vertical = vertical;
                fX = forward.x;
                fY = forward.y;
                fZ = forward.z;
                rX = right.x;
                rY = right.y;
                rZ = right.z;
                messageNumber = msgNumber;
            }

            public float horizontal;
            public float vertical;

            public float rX, rY, rZ;
            public float fX, fY, fZ;
            public uint messageNumber;

            public Vector3 Forward
            {
                get
                {
                    return new Vector3(fX, fY, fZ);
                }
            }

            public Vector3 Right
            {
                get
                {
                    return new Vector3(rX, rY, rZ);
                }
            }
        }

        private uint lastMessage = 0;
        private int counterOffset;
        private MoveMessage lastMoveMessage;
        private Vector3 lastMoveDirection;

        protected override void StartServer()
        {
            counterOffset = 0;
        }

        protected override void UpdateServer()
        {
            counterOffset = (counterOffset + 1) % 2;
            Character.transform.position += new Vector3(0f, 0.000000001f, 0f) * (counterOffset * 2 - 1);

            Transform parent = Character.transform.parent;
            Character.transform.SetParent(null);
            Character.SimpleMove(lastMoveDirection);
            Character.transform.SetParent(parent);
        }

        public void MoveCharacter(float horizontal, float vertical, Vector3 cameraFoward, Vector3 cameraRight)
        {
            if (serverObject.onServer)
            {
                var moveDirection = speed * (vertical * cameraFoward + horizontal * cameraRight);
                lastMoveDirection = moveDirection;
            }
            else
            {
                var message = new MoveMessage(horizontal, vertical, cameraFoward, cameraRight, ++lastMessage);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var moveMessage = ByteSerializer.FromBytes<MoveMessage>(data);
            if (moveMessage.messageNumber > lastMessage)
            {
                lastMessage = moveMessage.messageNumber;
                MoveCharacter(moveMessage.horizontal, moveMessage.vertical, moveMessage.Forward, moveMessage.Right);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //Debug.Log($"Hit! {hit.collider.gameObject.name}");
        }
    }
}
