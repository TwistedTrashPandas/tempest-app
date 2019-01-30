﻿using MastersOfTempest.Environment;
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
        public float radiusCollider = 50f;

        private CapsuleCollider winCondition;
        private bool toggleWinText;

        private GUIContent guiContent;
        private GUIStyle guiStyle;

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

            guiStyle.normal.textColor = new Color(0.36f, 0.34f, 0f);
        }

        public void OnWinServer()
        {
            byte[] buffer = new byte[1];
            buffer[0] = 1;
            GetComponent<Gamemaster>().GetEnvironmentManager().envSpawner.RemoveAllObjects();
            SendToAllClients(buffer, Facepunch.Steamworks.Networking.SendType.Reliable);
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
            while (guiStyle.fontSize < 250)
            {
                guiStyle.fontSize += 1;
                yield return new WaitForEndOfFrame();
            }
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
