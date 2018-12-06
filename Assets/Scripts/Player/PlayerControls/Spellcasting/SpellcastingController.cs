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

        private bool isActive;
        private List<SpellElement> spellElements = new List<SpellElement>();
        private const string SpellElementPrefabPath = "UIPrefabs/Wizard/SpellElement";
        private const int SpellComplexity = 4;
        private const KeyCode CastSpellKey = KeyCode.Space;
        private CanvasGroup canvasGroup;

        public bool Active
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                canvasGroup.alpha = isActive ? 1f : 0f;
                canvasGroup.blocksRaycasts = isActive;
                canvasGroup.interactable = isActive;
                spellElements.ForEach(element => element.enabled = isActive);
            }
        }

        private void Start()
        {
            //Spawn spell elements on the canvas and hide them
            var uiManager = UIManager.GetInstance();
            SetupCanvasGroup(uiManager.MainCanvas);
            var width = uiManager.MainCanvas.pixelRect.width / SpellComplexity;
            for (int i = 0; i < SpellComplexity; ++i)
            {
                spellElements.Add(uiManager.SpawnUIElement<SpellElement>(SpellElementPrefabPath));
                var pos = spellElements[i].RectTransform.anchoredPosition;
                pos.x = width / 2 + i * width;
                spellElements[i].RectTransform.anchoredPosition = pos;
                spellElements[i].canvas = uiManager.MainCanvas;
                spellElements[i].transform.SetParent(canvasGroup.transform);
            }
            Active = false;
        }

        /// <summary>
        /// Creates CanvasGroup object on the canvas. Used to hide/show UI elements for spell casting
        /// </summary>
        /// <param name="managerInstance"></param>
        private void SetupCanvasGroup(Canvas canvas)
        {
            var obj = new GameObject("CanvasGroupForSpells");
            obj.transform.SetParent(canvas.transform, false);
            canvasGroup = obj.AddComponent<CanvasGroup>();
            var rectTransform = obj.AddComponent<RectTransform>();
            var rect = rectTransform.rect;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        private void Update()
        {
            if (Active)
            {
                if (Input.GetKeyDown(CastSpellKey))
                {
                    TryCurrentCombination();
                }
            }
        }

        private void TryCurrentCombination()
        {
            var squence = spellElements.OrderBy(element => element.transform.position.x).Select(el => el.CurrentRune).ToArray();
            Spell fittingSpell = null;
            foreach (var spell in SpellList.Spells)
            {
                bool fail = false;
                for (int i = 0; i < squence.Length; ++i)
                {
                    if (squence[i] != spell.SpellSequence[i])
                    {
                        fail = true;
                        break;
                    }
                }
                if (!fail)
                {
                    fittingSpell = spell;
                    break;
                }
            }
            if (fittingSpell != null)
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
            Debug.Log("Success");
        }

        private void NegativeFeedback()
        {
            //TODO: Particle efffects? Timeout?
            Debug.Log("Fail");
        }
    }
}
