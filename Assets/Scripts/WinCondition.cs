using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MastersOfTempest
{
    public class WinCondition : NetworkBehaviour
    {
        public delegate void WinAnimation(GameObject ship);
        public static event WinAnimation OnWin;
        
        public float radiusCollider = 10f;
        private CapsuleCollider winCondition;

        protected override void StartServer()
        {
            base.StartServer();
            StartCoroutine(WinAfter10secs());
        }
        
        public void OnWinServer()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 1;
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            if (data[0] > 0)
                OnWin(gameObject.GetComponent<Gamemaster>().GetShip().gameObject);
        }

        private IEnumerator WinAfter10secs()
        {
            yield return new WaitForSeconds(10f);
            VectorField vectorField = GetComponent<Gamemaster>().GetEnvironmentManager().vectorField;
            winCondition = gameObject.AddComponent<CapsuleCollider>();
            winCondition.center = vectorField.GetCenterWS();
            winCondition.height = vectorField.GetCellSize() * vectorField.GetDimensions().y * 1.5f;
            winCondition.direction = 1;
            winCondition.isTrigger = true;
            winCondition.radius = 10f;
            if (serverObject.onServer)
            {
                OnWinServer();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // WIN EVENT
            if(other.tag == "Ship")
            {
                if (serverObject.onServer)
                    OnWin(other.transform.parent.gameObject);
                Debug.Log("Victory!");
            }
        }
    }
}
