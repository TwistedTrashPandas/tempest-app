using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Startscreen : MonoBehaviour
{
    public Button startB, quitB, tutorialB;
    void Start()
    {
        startB.onClick.AddListener(StartGame);
        quitB.onClick.AddListener(EndGame);
        tutorialB.onClick.AddListener(StartTutorial);
    }

   private void StartGame()
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    private void StartTutorial()
    {
        Debug.Log("Tutorial started");
    }

    private void EndGame()
    {
        Debug.Log("I am the destroyer of worlds! I decide over fate and death! I... am the Quit Button!");
        Application.Quit();
    }
}
