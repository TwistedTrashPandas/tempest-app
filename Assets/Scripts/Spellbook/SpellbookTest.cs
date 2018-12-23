using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest
{
    public class SpellbookTest : MonoBehaviour
    {
        public Spellbook spellbook;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                spellbook.OpenOrClose();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                spellbook.NextPage();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                spellbook.PreviousPage();
            }
        }
    }
}
