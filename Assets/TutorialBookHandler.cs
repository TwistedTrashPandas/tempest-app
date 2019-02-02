using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.F))
        {
                if(helpImage)
                {
                    helpImage = false;
                    Helpimage.SetActive(false);
                }

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
    }
    }

}
