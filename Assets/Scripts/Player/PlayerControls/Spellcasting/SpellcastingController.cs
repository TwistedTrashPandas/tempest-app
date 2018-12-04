using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    /// <summary>
    /// This controller is responsible for the spellcasting part of the 
    /// Wizard gameplay. Manages displaying, sorting of the "magical" objects
    /// and triggering the spell
    /// </summary>
    public class SpellcastingController : MonoBehaviour
    {
        public event EventHandler SpellCasted;
        //TODO: set the visibility based on the Active
        public bool Active { get; set; }
        private List<SpellElement> spellElements = new List<SpellElement>();
        private const string SpellElementPrefabPath = "";
        private const int SpellComplexity = 4;
        private const KeyCode CastSpellKey = KeyCode.Space;
        private void Start()
        {
            var uiManager = UIManager.GetInstance();
            var width = uiManager.MainCanvas.pixelRect.width / SpellComplexity;
            for(int i = 0; i < SpellComplexity; ++i)
            {
                spellElements.Add(uiManager.SpawnUIElement<SpellElement>(SpellElementPrefabPath));
                var pos = spellElements[i].RectTransform.anchoredPosition;
                pos.x = width/2 + i*width;
                spellElements[i].RectTransform.anchoredPosition = pos;
                //TODO: perhaps randomize current Rune
            }
        }

        private void Update()
        {
            if (Active)
            {
                if(Input.GetKeyDown(CastSpellKey))
                {
                    TryCurrentCombination();
                }
            }
        }

        private void TryCurrentCombination()
        {
            var squence = spellElements.OrderBy(element => element.transform.position.x).Select(el => el.CurrentRune).ToArray();
            Spell fittingSpell = null;
            foreach(var spell in SpellList.Spells)
            {
                bool fail = false;
                for(int i = 0; i < squence.Length; ++i)
                {
                    if(squence[i] != spell.SpellSequence[i])
                    {
                        fail = true;
                        break;
                    }
                }
                if(!fail)
                {
                    fittingSpell = spell;
                    break;
                }
            }
            if(fittingSpell != null)
            {
                SpellCasted?.Invoke(this, new SpellCastedEventArgs(fittingSpell));
                PositiveFeedback();
            }
            else 
            {
                NegativeFeedback();
            }
        }

        private void PositiveFeedback()
        {
            //TODO: Particle efffects?
            //TODO: draw connections between the elements
        }

        private void NegativeFeedback()
        {
            //TODO: Particle efffects? Timeout?
        }
    }
}
