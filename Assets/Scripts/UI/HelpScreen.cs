using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpScreen : MonoBehaviour
{
    public KeyCode keyToPress = KeyCode.F1;
    public Image help;
    public Image hint;

    private Text hintText;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        hintText = hint.GetComponentInChildren<Text>();

        StartCoroutine(ShowHint(3));
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
