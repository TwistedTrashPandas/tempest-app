using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;

namespace MastersOfTempest
{
    public class CubeNetworkBehaviour : NetworkBehaviour
    {
        [SerializeField]
        private float inputsPerSec = 1;
        [SerializeField]
        private float movementSpeed = 1;
        [SerializeField]
        private float jumpHeight = 1;

        private float horizontalInput = 0;
        private float verticalInput = 0;
        private bool jump = false;

        private struct CubeNetworkMessage
        {
            public float horizontalInput;
            public float verticalInput;
            public bool jump;

            public CubeNetworkMessage (float horizontalInput, float verticalInput, bool jump)
            {
                this.horizontalInput = horizontalInput;
                this.verticalInput = verticalInput;
                this.jump = jump;
            }
        };

        protected override void StartClient()
        {
            StartCoroutine(SendInputToServer());
        }

        protected override void UpdateClient()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CubeNetworkMessage message = new CubeNetworkMessage(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), true);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
        }

        IEnumerator SendInputToServer ()
        {
            while (true)
            {
                CubeNetworkMessage message = new CubeNetworkMessage(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), false);
                SendToServer(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Unreliable);

                yield return new WaitForSeconds(1.0f / inputsPerSec);
            }
        }

        protected override void UpdateServer()
        {
            Vector3 force = movementSpeed * Time.deltaTime * (Vector3.forward * verticalInput + Vector3.right * horizontalInput);
            GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            CubeNetworkMessage cubeNetworkMessage = ByteSerializer.FromBytes<CubeNetworkMessage>(data);
            horizontalInput = cubeNetworkMessage.horizontalInput;
            verticalInput = cubeNetworkMessage.verticalInput;
            jump = cubeNetworkMessage.jump;

            if (jump)
            {
                Vector3 jumpForce = jumpHeight * Vector3.up;
                GetComponent<Rigidbody>().AddForce(jumpForce, ForceMode.VelocityChange);
            }
        }
    }
}
