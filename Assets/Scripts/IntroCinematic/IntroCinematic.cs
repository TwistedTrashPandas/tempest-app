using Cinemachine;
using Facepunch.Steamworks;
using MastersOfTempest.ShipBL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroCinematic : MonoBehaviour
{
    public CinemachineVirtualCamera[] cinemachineVirtualCameras;
    public float timeUntilLevelLoad = 5;
    public string clientSceneName = "Client";
    public string serverSceneName = "Server";

    public void Configure ()
    {
        Transform ship = FindObjectOfType<Ship>().transform;

        foreach (CinemachineVirtualCamera c in cinemachineVirtualCameras)
        {
            c.LookAt = ship;
            c.Follow = ship;
        }

        StartCoroutine(LoadLevel());
    }

    private IEnumerator LoadLevel ()
    {
        yield return new WaitForSeconds(timeUntilLevelLoad);

        // Load client scene
        SceneManager.LoadScene(clientSceneName);

        // Also load server scene if you are the owner of the lobby
        if (Client.Instance.Lobby.Owner == Client.Instance.SteamId)
        {
            SceneManager.LoadScene(serverSceneName, LoadSceneMode.Additive);
        }
    }
}
