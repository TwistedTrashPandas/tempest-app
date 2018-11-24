using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MastersOfTempest.Networking;

namespace MastersOfTempest
{
    public class PushRigidbodyUp : NetworkBehaviour
    {
        protected override void StartServer()
        {
            gameObject.AddComponent<Rigidbody>();
        }

        protected override void UpdateClient()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit[] hits = Physics.RaycastAll(ray, 100);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.Equals(transform))
                    {
                        SendToServer("Hey server, please push me up!");
                    }
                }
            }
        }

        protected override void OnClientReceivedMessage(string message, ulong steamID)
        {
            Debug.Log(message);
        }

        protected override void OnServerReceivedMessage(string message, ulong steamID)
        {
            Debug.Log(message);

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.AddForce(new Vector3(0, 10, 0), ForceMode.Impulse);
            rigidbody.AddTorque(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));

            SendToClient(steamID, "Hey client, I pushed your rigidbody " + rigidbody + " up!");
        }
    }
}
