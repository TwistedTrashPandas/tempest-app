using UnityEngine;
using System.Collections.Generic;

namespace MastersOfTempest.Glow
{
    public class GlowObjectCmd : MonoBehaviour
    {
        public Color GlowColor;
        public float LerpFactor = 10;

        public Renderer[] Renderers
        {
            get;
            private set;
        }

        public Color CurrentColor
        {
            get { return _currentColor; }
        }

        private Color _currentColor = Color.black;
        private Color _targetColor;

        void Start()
        {
            Renderers = GetComponentsInChildren<Renderer>();
            enabled = false;
        }

        public void TurnTheGlowOn()
        {
            _targetColor = GlowColor;
            if (enabled == false)
            {
                if (ColorsEqual(_currentColor, Color.black))
                {
                    GlowController.RegisterObject(this);
                }
                GlowController.RegisterColorChange(this);
            }
            enabled = true;
        }

        public void TurnTheGlowOff()
        {
            _targetColor = Color.black;
            if (enabled == false && !ColorsEqual(_currentColor, Color.black))
            {
                GlowController.RegisterColorChange(this);
                enabled = true;
            }
        }

        /// <summary>
        /// Update color, disable self if we reach our target color.
        /// </summary>
        private void Update()
        {
            _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * LerpFactor);

            if (ColorsEqual(_currentColor, _targetColor))
            {
                enabled = false;
                //Remove the object from active color change list
                GlowController.DeregisterColorChange(this);
                //Remove this object from consideration if it is not supposed to be glowing at all
                if (ColorsEqual(_currentColor, Color.black))
                {
                    GlowController.DeregisterObject(this);
                }
            }
        }

        private bool ColorsEqual(Color a, Color b)
        {
            return Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) && Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a);
        }
    }
}
