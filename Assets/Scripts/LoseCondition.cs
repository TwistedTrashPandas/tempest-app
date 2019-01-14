using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MastersOfTempest
{
    public class LoseCondition : NetworkBehaviour
    {
        public delegate void LoseAnimation();
        public static event LoseAnimation OnLose;
        public float overallDestructionThreshold = 0.9f;
        private PostProcessVolume postProcessVolume;

        private ShipBL.ShipPartManager shipPartManager;

        protected override void StartServer()
        {
            base.StartServer();
            //StartCoroutine(WinAfter10secs());
            StartCoroutine(GetShipPartManager());
        }

        protected override void StartClient()
        {
            base.StartClient();
            postProcessVolume = FindObjectOfType<PostProcessVolume>();
            //StartCoroutine(ScreenSaturation(postProcessVolume.profile.GetSetting<ColorGrading>()));
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
            {
                ClientLose();
            }
        }

        private void ClientLose()
        {
            StartCoroutine(ScreenSaturation(postProcessVolume.profile.GetSetting<ColorGrading>()));
            //OnLose();
        }

        private IEnumerator ScreenSaturation(ColorGrading colorGrading)
        {
            while (colorGrading.saturation.GetValue<float>() > -100f)
            {
                colorGrading.saturation.Override(colorGrading.saturation.GetValue<float>() - 0.5f);
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0.1f, Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

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

        private void OnApplicationQuit()
        {
            postProcessVolume.profile.GetSetting<ColorGrading>().saturation.Override(0f);
        }
    }
}
