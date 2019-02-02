using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MastersOfTempest
{
    public class TutorialBookHandler : MonoBehaviour
{
 
        public Spellbook spellbook;
        public GameObject Helpimage;

        private bool helpImage = true;

        void Start()
        {
            spellbook.OpenOrClose();
        }

        void Update()
    {
        if(Input.anyKey)
        {
                if (helpImage)
                {
                    helpImage = false;
                    Helpimage.SetActive(false);
                }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
             

                spellbook.OpenOrClose();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            spellbook.NextPage();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            spellbook.PreviousPage();
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
                SceneManager.LoadScene("StartScene", LoadSceneMode.Single);
        }

        }
    }

}
