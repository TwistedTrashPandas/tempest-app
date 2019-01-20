using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ESCScreen : MonoBehaviour
{
    public static ESCScreen Instance = null;

    public KeyCode keyToPress = KeyCode.Escape;

    private Canvas canvas;

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            Toggle();
        }
    }

    public void LoadScene (string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Toggle()
    {
        canvas.enabled = !canvas.enabled;
    }
}
