using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;

//  1. Initializes steam on startup
//  2. Calls Update
//  3. Disposes and shuts down Steam on close

public class ClientManager : MonoBehaviour
{
    // The app id should be 480 for testing purposes
    public uint appId = 480;

    private Client client;

	void Awake ()
    {
        // Make sure to have this object active across all scenes
        DontDestroyOnLoad(gameObject);
        Config.ForUnity(Application.platform.ToString());

        try
        {
            // Create a steam_appid.txt with the app id in it, required by the SDK
            System.IO.File.WriteAllText(Application.dataPath + "/../steam_appid.txt", appId.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Couldn't write steam_appid.txt: " + e.Message);
        }

        // Create the client
        client = new Client(appId);

        if (client.IsValid)
        {
            Debug.Log("Steam Initialized: " + client.Username + " / " + client.SteamId);
        }
        else
        {
            client = null;
            Debug.LogWarning("Couldn't initialize Steam. Make sure that Steam is running.");
        }
	}
	
	void Update()
    {
        if (client != null)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Steam Update");
            client.Update();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Dispose();
            client = null;
        }
    }
}
