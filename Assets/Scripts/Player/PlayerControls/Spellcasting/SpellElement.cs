using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    /// <summary>
    /// This is the <see cref="MonoBehaviour"/> of the objects
    /// that Wizard has to manipulate on the screen
    /// </summary>
    public class SpellElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Canvas canvas;
        public Rune CurrentRune { get; set; }

        public RectTransform RectTransform { get; private set; }
        private const int LeftMouseButton = 0;
        private const int RightMouseButton = 1;
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            if (RectTransform == null)
            {
                throw new InvalidOperationException($"{nameof(RectTransform)} is not specified!");
            }
            if (canvas == null)
            {
                throw new InvalidOperationException($"{nameof(canvas)} is not specified!");
            }
        }

        private float holdDownTimer = 0f;
        private const float ChangeTime = 2f;
        private void Update()
        {
            if (!dragging)
            {
                if (changing)
                {
                    if (Input.GetMouseButton(RightMouseButton))
                    {
                        holdDownTimer += Time.deltaTime;
                        if (holdDownTimer > ChangeTime)
                        {
                            changing = false;
                            ChangeCurrentRune();
                        }
                    }
                    else
                    {
                        changing = false;
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(RightMouseButton) && !dragging)
                    {
                        holdDownTimer = 0f;
                        changing = true;
                    }
                }
            }
        }

        private void ChangeCurrentRune()
        {
            //TODO: actually change rune
            Debug.Log("Change called");
        }

        private bool dragging;
        private bool changing;
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Started to drag!");
            dragging = Input.GetMouseButton(LeftMouseButton) && !changing;
            Debug.Log($"Started to drag = {dragging}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging)
            {
                RectTransform.anchoredPosition = Input.mousePosition / canvas.scaleFactor;
                Debug.Log($"X: {Input.mousePosition.x}; Y: {Input.mousePosition.y}");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }
    }
}
