using MastersOfTempest.Environment;
using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MastersOfTempest
{
    public class WinCondition : NetworkBehaviour
    {
        public delegate void WinAnimation(GameObject ship);
        public static event WinAnimation OnWin;

        public Font winFont;
        public float radiusCollider = 80f;

        public float timeZoom;
        public Material skybox;

        private float timeZoomCurr;

        private CapsuleCollider winCondition;
        private bool toggleWinText;

        private GUIContent guiContent;
        private GUIStyle guiStyle;

        private Transform targetLookAt;
        private Vector3 targetCamPos;
        private Vector3 startCamPos;

        protected override void StartServer()
        {
            base.StartServer();
            StartCoroutine(InitAfter5Seconds());
        }

        protected override void StartClient()
        {
            base.StartServer();
            toggleWinText = false;
            guiContent = new GUIContent("YOU WON");
            guiStyle = new GUIStyle();

            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.fontSize = 10;
            guiStyle.font = winFont;

            targetCamPos = new Vector3();
            timeZoomCurr = 0f;

            guiStyle.normal.textColor = new Color(0.36f, 0.34f, 0f);
        }

        public void OnWinServer()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 1;
            GetComponent<Gamemaster>().GetEnvironmentManager().envSpawner.RemoveAllObjects();
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void Update()
        {
            if (toggleWinText && timeZoomCurr < timeZoom)
            {
                Camera.main.transform.position = Vector3.Lerp(startCamPos, targetCamPos, timeZoomCurr / timeZoom);
                timeZoomCurr += Time.deltaTime;
                Camera.main.transform.LookAt(targetLookAt);
            }
        }

        private void OnGUI()
        {
            if (toggleWinText)
            {
                float up = 60f * Screen.height / Screen.height;
                float right = 60f * Screen.width / Screen.width;
                GUI.Label(new Rect(Screen.width / 2f - right, Screen.height / 2f - up, Screen.width / 10f, Screen.height / 10f), guiContent, guiStyle);

                /*
                GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
                buttonStyle.fontSize = (int)(40 * Screen.width / 1920f);
                buttonStyle.font = loseFont;

                if (GUI.Button(new Rect(Screen.width * 0.75f, Screen.height * 0.8f, Screen.width / 6f, Screen.height / 18f), "Return to Lobby", buttonStyle))
                {
                    SceneManager.LoadScene(0);
                }*/
            }
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            base.OnClientReceivedMessageRaw(data, steamID);
            if (data[0] > 0)
            {
                if (!toggleWinText)
                {
                    startCamPos = Camera.main.transform.position;
                    targetLookAt = GetComponent<Gamemaster>().GetShip().transform;
                    targetCamPos.y = 1.2f * targetLookAt.position.y;
                    targetCamPos.x = -200f;
                    targetCamPos.z = -200f;
                    toggleWinText = true;
                    OnWin(gameObject.GetComponent<Gamemaster>().GetShip().gameObject);
                    StartCoroutine(IncreaseFontSize());
                }
            }
        }

        protected override void OnTriggerEnter(Collider c)
        {
            if (c.gameObject.tag == "Ship")
                OnWinServer();
        }

        private IEnumerator IncreaseFontSize()
        {
            float startFogDens = RenderSettings.fogDensity;
            while (guiStyle.fontSize < 250)
            {
                guiStyle.fontSize += 1;
                if (RenderSettings.fog)
                {
                    RenderSettings.fogDensity -= 0.01f;
                    RenderSettings.skybox = skybox;
                }

                if (RenderSettings.fogDensity < 0f)
                {
                    RenderSettings.fog = false;
                    RenderSettings.fogDensity = startFogDens;
                }
                yield return new WaitForEndOfFrame();
            }
            RenderSettings.fogDensity = startFogDens;
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
