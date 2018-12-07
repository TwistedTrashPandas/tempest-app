using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    /// <summary>
    /// Shows hints to the wizard players about 
    /// available spells and their components
    /// </summary>
    public class SpellReferenceInfo : MonoBehaviour
    {
        public bool Active { get; set; } = true;
        private string spellReferenceTextPrefabName = "UIPrefabs/Wizard/SpellReferenceText";
        private TMP_Text text;
        private List<Spell> spells;
        private int currentSpell = 0;
        private void Start()
        {
            text = UIManager.GetInstance().SpawnUIElement<TMP_Text>(spellReferenceTextPrefabName);
            spells = SpellList.Spells;
            ShowCombination(currentSpell);
        }

        private void Update()
        {
            if(Active)
            {
                if(Input.GetAxis("Mouse ScrollWheel") != 0f)
                {
                    currentSpell += 1 * Mathf.CeilToInt(Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")));
                    if(currentSpell < 0)
                    {
                        currentSpell = spells.Count - 1;
                    }
                    else if(currentSpell == spells.Count)
                    {
                        currentSpell = 0;
                    }
                    ShowCombination(currentSpell);
                }
            }
        }

        private void ShowCombination(int index)
        {
            text.text = $"To cast {spells[index].Name}: {string.Join(", ", spells[index].SpellSequence.Select(rune => rune.FriendlyName()))}";
        }
    }
}
