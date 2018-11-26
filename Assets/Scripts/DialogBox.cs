using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MasterOfTempest
{
    // Creates simple dialog boxes at runtime with yes/no buttons that invoke functions when they are pressed
    public class DialogBox : MonoBehaviour
    {
        public delegate void OnButtonClick();

        public Text text;
        public OnButtonClick onYes;
        public OnButtonClick onNo;

        public static void Show(string text, OnButtonClick onYes, OnButtonClick onNo)
        {
            Canvas canvas = FindObjectOfType<Canvas>();

            if (canvas != null)
            {
                GameObject dialogBoxPrefab = Resources.Load<GameObject>("Dialog Box");
                DialogBox dialogBox = Instantiate(dialogBoxPrefab, canvas.transform).GetComponent<DialogBox>();
                dialogBox.text.text = text;
                dialogBox.onYes = onYes;
                dialogBox.onNo = onNo;
            }
            else
            {
                Debug.LogError("There is no canvas in the scene that the dialog box can be attached to!");
            }
        }

        public void Yes()
        {
            if (onYes != null)
            {
                onYes.Invoke();
            }

            Destroy(gameObject);
        }

        public void No()
        {
            if (onNo != null)
            {
                onNo.Invoke();
            }

            Destroy(gameObject);
        }
    }
}
