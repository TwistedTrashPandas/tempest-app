using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpScreen : MonoBehaviour
{
    public static HelpScreen Instance = null;

    public KeyCode keyToPress = KeyCode.F1;
    public Image help;
    public Image hint;

    private Text hintText;

    protected void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            // Replace the already created hint screen with this one (otherwise the hint animation would not play when reloaded)
            Destroy(Instance.gameObject);
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        hintText = hint.GetComponentInChildren<Text>();

        StartCoroutine(ShowHint(5));
    }

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            hint.gameObject.SetActive(false);
            help.gameObject.SetActive(!help.gameObject.activeSelf);
        }
    }

    IEnumerator ShowHint (float time)
    {
        hint.gameObject.SetActive(true);

        yield return new WaitForSeconds(time / 2);

        hint.CrossFadeColor(new Color(0, 0, 0, 0), time / 2, false, true);
        hintText.CrossFadeAlpha(0, time / 2, false);

        yield return new WaitForSeconds(time / 2);

        hint.gameObject.SetActive(false);
    }

    public static void Instantiate ()
    {
        Instantiate(Resources.Load<GameObject>("Help Screen Canvas"));
    }
}
