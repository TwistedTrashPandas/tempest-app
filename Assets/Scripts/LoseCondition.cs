using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using MastersOfTempest.PlayerControls;
using UnityEngine.SceneManagement;

namespace MastersOfTempest
{
    public class LoseCondition : NetworkBehaviour
    {
        public delegate void LoseAnimation();
        public static event LoseAnimation OnLose;
        public float overallDestructionThreshold = 0.9f;
        public Font loseFont;

        private GUIContent guiContent;
        private GUIStyle guiStyle;
        private bool toggleLossText;
        private PostProcessVolume postProcessVolume;
        private ShipBL.ShipPartManager shipPartManager;

        protected override void StartServer()
        {
            base.StartServer();
            StartCoroutine(GetShipPartManager());
            // StartCoroutine(LostAfterSecs(5f));
        }

        protected override void StartClient()
        {
            base.StartClient();
            postProcessVolume = FindObjectOfType<PostProcessVolume>();
            toggleLossText = false;
            guiContent = new GUIContent("YOU DIED");
            guiStyle = new GUIStyle();

            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.fontSize = 10;
            guiStyle.font = loseFont;

            guiStyle.normal.textColor = new Color(0.6f, 0f, 0f);
            //ClientLose();
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

        private IEnumerator LostAfterSecs(float time_x)
        {
            yield return new WaitForSecondsRealtime(time_x);
            OnLoseServer();
        }

        public void OnLoseServer()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 1;
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
            //StartCoroutine(TimeScale());
            StartCoroutine(StartShipExplosion(GameClient.Instance.GetPing()));
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            // message consists of one byte to tell if you won or lost
            base.OnClientReceivedMessageRaw(data, steamID);
            if (data[0] > 0)
            {
                ClientLose();
            }
        }

        private void ClientLose()
        {
            print(postProcessVolume.gameObject.name);
            print(postProcessVolume.profile.GetSetting<ColorGrading>());
            StartCoroutine(ScreenSaturation(postProcessVolume.profile.GetSetting<ColorGrading>()));
            toggleLossText = !toggleLossText;
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
            StartCoroutine(TimeScale());
            //OnLose();
        }

        private void OnGUI()
        {
            if (toggleLossText)
            {
                GUI.Label(new Rect(Screen.width / 2f - 80f, Screen.height / 2f - 100f, Screen.width / 10f, Screen.height / 10f), guiContent, guiStyle);

                GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
                buttonStyle.fontSize = 40;
                buttonStyle.font = loseFont;

                if (GUI.Button(new Rect(Screen.width * 0.75f, Screen.height * 0.8f, Screen.width / 6f, Screen.height / 18f), "Return to Lobby", buttonStyle))
                {
                    SceneManager.LoadScene("Lobby");
                }
            }
        }

        private IEnumerator StartShipExplosion(float timeToWait)
        {
            yield return new WaitForSecondsRealtime(timeToWait);
            //OnLoseServer();
            GetComponent<Gamemaster>().GetShip().DestroyShip();
        }

        private IEnumerator ScreenSaturation(ColorGrading colorGrading)
        {
            while (colorGrading.saturation.GetValue<float>() > -100f || guiStyle.fontSize < 250)
            {
                colorGrading.saturation.Override(Mathf.Max(colorGrading.saturation.GetValue<float>() - 1.5f, -100f));
                guiStyle.fontSize += 1;
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator TimeScale()
        {
            while (Time.timeScale > 0.01f)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 0.01f, Time.unscaledDeltaTime);

                yield return new WaitForEndOfFrame();
            }
        }

        public void CheckOverAllDestruction()
        {
            if (shipPartManager != null)
            {
                float[] shipDestruction = shipPartManager.CalculateOverallDestruction();
                int destructedParts = 0;
                for (int i = 0; i < shipDestruction.Length; i++)
                {
                    if (shipDestruction[i] >= overallDestructionThreshold)
                    {
                        destructedParts++;
                    }
                }
                if (destructedParts > 1)
                    OnLoseServer();
            }
            else
            {
                throw new System.InvalidOperationException("ShipPartManager not found/set.");
            }
        }

        private void OnApplicationQuit()
        {
            //postProcessVolume.profile.GetSetting<ColorGrading>().saturation.Override(0f);
        }
    }
}
