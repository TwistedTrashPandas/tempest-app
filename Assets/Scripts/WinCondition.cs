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
        
        public float radiusCollider = 50f;
        private CapsuleCollider winCondition;

        protected override void StartServer()
        {
            base.StartServer();
            StartCoroutine(InitAfter5Seconds());
        }
        
        public void OnWinServer()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 1;
            GetComponent<Gamemaster>().GetEnvironmentManager().envSpawner.RemoveAllObjects();
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            if (data[0] > 0)
                OnWin(gameObject.GetComponent<Gamemaster>().GetShip().gameObject);
        }

        protected override void OnTriggerEnter(Collider c)
        {
            if (c.gameObject.tag == "Ship")
                OnWinServer();
        }

        private IEnumerator InitAfter5Seconds()
        {
            yield return new WaitForSeconds(5f);
            if (serverObject.onServer)
            {
                VectorField vectorField = GetComponent<Gamemaster>().GetEnvironmentManager().vectorField;
                winCondition = gameObject.AddComponent<CapsuleCollider>();
                winCondition.center = vectorField.GetCenterWS();
                winCondition.height = vectorField.GetCellSize() * vectorField.GetDimensions().y * 2f;
                winCondition.direction = 1;
                winCondition.isTrigger = true;
                winCondition.radius = radiusCollider;
            }
        }
    }
}
