using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MastersOfTempest
{
    public class LoseCondition : NetworkBehaviour
    {
        public delegate void LoseAnimation();
        public static event LoseAnimation OnLose;
        public float overallDestructionThreshold;
        private ShipBL.ShipPartManager shipPartManager;

        protected override void StartServer()
        {
            base.StartServer();
            //StartCoroutine(WinAfter10secs());
            StartCoroutine(GetShipPartManager());
        }

        private IEnumerator GetShipPartManager()
        {
            while (shipPartManager == null)
            {
                yield return new WaitForEndOfFrame();
                if (GetComponent<Gamemaster>().GetShip() != null)
                    shipPartManager = GetComponent<Gamemaster>().GetShip().GetShipPartManager();
            }
        }

        public void OnLoseServer()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 1;
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            if (data[0] > 0)
                OnLose();
        }

        public void CheckOverAllDestruction()
        {
            if (shipPartManager != null)
            {
                if (shipPartManager.CalculateOverallDestruction() > overallDestructionThreshold)
                    OnLoseServer();
            }
            else
            {
                throw new System.InvalidOperationException("ShipPartManager not found/set.");
            }
        }
    }
}
