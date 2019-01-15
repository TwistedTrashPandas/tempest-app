using System;
using System.Collections;
using System.Collections.Generic;
using MastersOfTempest.Networking;
using UnityEngine;

namespace MastersOfTempest.PlayerControls.Spellcasting
{
    /// <summary>
    /// Changes the material main color based on casted spells
    /// </summary>
    public class SpellDependantColorManager : NetworkBehaviour
    {
        [Serializable]
        private struct SetColorMessage
        {
            public float r, g, b, a;
            public Color Color
            {
                get
                {
                    return new Color(r, g, b, a);
                }
                set
                {
                    r = value.r;
                    g = value.g;
                    b = value.b;
                    a = value.a;
                }
            }

            public SetColorMessage(Color value)
            {
                r = value.r;
                g = value.g;
                b = value.b;
                a = value.a;
            }
        }

        public MeshRenderer mat;

        private SpellcastingController spellcastingController;
        private Color originalColor;
        private const float FullCycleTime = 1f;

        protected override void StartServer()
        {
            spellcastingController = FindObjectOfType<SpellcastingController>();
            if (spellcastingController == null)
            {
                throw new InvalidOperationException($"{nameof(spellcastingController)} is not specified!");
            }
            spellcastingController.SpellCasted += OnSpellCasted;
        }

        protected override void StartClient()
        {
            if (mat == null)
            {
                throw new InvalidOperationException($"{nameof(mat)} is not specified!");
            }
            originalColor = mat.material.color;
        }

        private void ChangeColor(Color color)
        {
            if (serverObject.onServer)
            {
                var message = new SetColorMessage(color);
                SendToAllClients(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
            else
            {
                if (globalCancellationToken != null)
                {
                    globalCancellationToken.CancellationRequested = true;
                }
                globalCancellationToken = new CoroutineCancellationToken();
                StartCoroutine(ChangeColorCoroutine(color, globalCancellationToken));
            }
        }

        private CoroutineCancellationToken globalCancellationToken;
        private IEnumerator ChangeColorCoroutine(Color color, CoroutineCancellationToken cancellationToken)
        {
            float timeElapsed = 0f;
            float halfTime = FullCycleTime / 2f;
            var startColor = mat.material.color;
            //Change color from the the current one to the desired
            while (timeElapsed < halfTime && !cancellationToken.CancellationRequested)
            {
                yield return null;
                mat.material.color = Color.Lerp(startColor, color, timeElapsed / halfTime);
                timeElapsed += Time.deltaTime;
            }
            timeElapsed = 0f;
            startColor = mat.material.color;
            //Change the color back to the original if cancellation was not requested
            while (timeElapsed < halfTime && !cancellationToken.CancellationRequested)
            {
                yield return null;
                mat.material.color = Color.Lerp(startColor, originalColor, timeElapsed / halfTime);
                timeElapsed += Time.deltaTime;
            }
            if(!cancellationToken.CancellationRequested)
            {
                globalCancellationToken = null;
                mat.material.color = originalColor;
            }
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var message = ByteSerializer.FromBytes<SetColorMessage>(data);
            ChangeColor(message.Color);
        }

        private void OnSpellCasted(object sender, EventArgs args)
        {
            var arguments = (SpellCastedEventArgs)args;
            ChangeColor(arguments.Spell.SpellColor);
        }
    }
}
