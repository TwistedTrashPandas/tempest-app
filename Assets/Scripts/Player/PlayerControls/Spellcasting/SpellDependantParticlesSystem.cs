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
    public class SpellDependantParticlesSystem : NetworkBehaviour
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

        public ParticleSystem ps;

        private SpellcastingController spellcastingController;


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
            if (ps == null)
            {
                throw new InvalidOperationException($"{nameof(ps)} is not specified!");
            }
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
                var main = ps.main;
                main.startColor = color;
                if (!ps.isPlaying)
                {
                    ps.Play();
                }
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
