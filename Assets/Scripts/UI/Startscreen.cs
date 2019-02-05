using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Startscreen : MonoBehaviour
{
    public Image downloadImage;
    public Button startB, quitB, tutorialB;
    void Start()
    {
        startB.onClick.AddListener(StartGame);
        quitB.onClick.AddListener(EndGame);
        tutorialB.onClick.AddListener(StartTutorial);

        StartCoroutine(LoadDownloadImage());
    }

   private void StartGame()
    {
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    private void StartTutorial()
    {
        SceneManager.LoadScene("TutorialScene", LoadSceneMode.Single);
    }

    private void EndGame()
    {
        Debug.Log("I am the destroyer of worlds! I decide over fate and death! I... am the Quit Button!");
        Application.Quit();
    }

    public IEnumerator LoadDownloadImage ()
    {
        WWW localFile;
        Texture texture;
        Sprite sprite;

        string finalPath = "file://" + Application.streamingAssetsPath + "/Download.png";
        localFile = new WWW(finalPath);

        yield return localFile;

        texture = localFile.texture;
        sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        downloadImage.sprite = sprite;
        downloadImage.preserveAspect = true;
    }
}
