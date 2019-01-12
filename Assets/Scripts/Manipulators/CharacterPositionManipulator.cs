using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;
using System;
using UnityEngine.SceneManagement;

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
                    Scene previouslyActiveScene = SceneManager.GetActiveScene();
                    Scene shit = GameServer.Instance.gameObject.scene;
                    SceneManager.SetActiveScene(shit);
                    var ships = FindObjectsOfType<ShipBL.Ship>();
                    shipPhysics = FindObjectOfType<ShipBL.Ship>().GetComponent<Rigidbody>();
                    SceneManager.SetActiveScene(previouslyActiveScene);
                }
                return shipPhysics;
            }
        }
        private const float speed = .1f;

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

        protected override void StartServer()
        {

        }

        public void MoveCharacter(float horizontal, float vertical, Vector3 cameraFoward, Vector3 cameraRight)
        {
            if (serverObject.onServer)
            {
                var moveDirection = speed * (vertical * cameraFoward + horizontal * cameraRight);
                Debug.Log($"Moving with speed: {moveDirection}");
                Character.SimpleMove(moveDirection);// - ShipPhysics.velocity));
            }
            else
            {
                var message = new MoveMessage(horizontal, vertical, cameraFoward, cameraRight, ++lastMessage);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Unreliable);
            }
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var message = ByteSerializer.FromBytes<MoveMessage>(data);
            if (message.messageNumber > lastMessage)
            {
                lastMessage = message.messageNumber;
                MoveCharacter(message.horizontal, message.vertical, message.Forward, message.Right);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Debug.Log($"Hit! {hit.collider.gameObject.name}");
            Debug.Log(hit.moveDirection);
        }
    }
}
