using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Startscreen : MonoBehaviour
{
    public Button startB, quitB;
    void Start()
    {
        startB.onClick.AddListener(StartGame);
        quitB.onClick.AddListener(EndGame);
       
    }

   private void StartGame()
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    private void EndGame()
    {
        Debug.Log("I am the destroyer of worlds! I decide over fate and death! I... am the Quit Button!");
        Application.Quit();
    }
}
