using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    /// <summary>
    /// This is the <see cref="MonoBehaviour"/> of the objects
    /// that Wizard has to manipulate on the screen
    /// </summary>
    public class SpellElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Canvas canvas;

        public Sprite FireSprite;
        public Sprite WaterSprite;
        public Sprite AirSprite;
        public Sprite IceSprite;

        public RectTransform RectTransform { get; private set; }

        private const float ChangeTime = 1f;
        private Image image;
        private Rune currentRune;
        private bool dragging;
        private bool changing;
        private float holdDownTimer = 0f;
        private bool mouseOver = false;
        private const int LeftMouseButton = 0;
        private const int RightMouseButton = 1;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            if (RectTransform == null)
            {
                throw new InvalidOperationException($"{nameof(RectTransform)} is not specified!");
            }
            image = GetComponent<Image>();
            if (image == null)
            {
                throw new InvalidOperationException($"{nameof(image)} is not specified!");
            }
        }
        private void Start()
        {
            if (canvas == null)
            {
                throw new InvalidOperationException($"{nameof(canvas)} is not specified!");
            }
            if (FireSprite == null)
            {
                throw new InvalidOperationException($"{nameof(FireSprite)} is not specified!");
            }
            if (WaterSprite == null)
            {
                throw new InvalidOperationException($"{nameof(WaterSprite)} is not specified!");
            }
            if (AirSprite == null)
            {
                throw new InvalidOperationException($"{nameof(AirSprite)} is not specified!");
            }
            if (IceSprite == null)
            {
                throw new InvalidOperationException($"{nameof(IceSprite)} is not specified!");
            }
            var runes = Enum.GetValues(typeof(Rune));
            CurrentRune = (Rune)runes.GetValue(UnityEngine.Random.Range(0, runes.Length));
        }

        public Rune CurrentRune
        {
            get
            {
                return currentRune;
            }
            set
            {
                currentRune = value;
                switch (currentRune)
                {
                    case Rune.Fire: image.sprite = FireSprite; break;
                    case Rune.Water: image.sprite = WaterSprite; break;
                    case Rune.Wind: image.sprite = AirSprite; break;
                    case Rune.Ice: image.sprite = IceSprite; break;
                    default: throw new InvalidOperationException($"Unexpected {nameof(Rune)} value of {currentRune}");
                }
            }
        }

        private void Update()
        {
            if (mouseOver && !dragging)
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
                        Debug.Log("Changing..");
                    }
                }
            }
        }

        private void ChangeCurrentRune()
        {
            //TODO: change rune intellegently
            var runes = Enum.GetValues(typeof(Rune));
            CurrentRune = (Rune)runes.GetValue(UnityEngine.Random.Range(0, runes.Length));
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = Input.GetMouseButton(LeftMouseButton) && !changing;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging)
            {
                RectTransform.anchoredPosition = Input.mousePosition / canvas.scaleFactor;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseOver = false;
        }
    }
}
