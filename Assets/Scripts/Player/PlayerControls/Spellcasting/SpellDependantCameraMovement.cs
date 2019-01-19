using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MastersOfTempest.Networking;
using MastersOfTempest.PlayerControls.Spellcasting;
using MastersOfTempest.ShipBL;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// Changes the material main color based on casted spells
    /// </summary>
    public class SpellDependantCameraMovement : NetworkBehaviour
    {
        [Serializable]
        private struct CameraMovement
        {
            public float x, y, z;
            public float i;

            public Vector3 Direciton
            {
                get
                {
                    return new Vector3(x, y, z);
                }
                set
                {
                    x = value.x;
                    y = value.y;
                    z = value.z;
                }
            }

            public CameraMovement(Vector3 direction, float intensity)
            {
                x = direction.x;
                y = direction.y;
                z = direction.z;
                this.i = intensity;
            }
        }
        private Ship ship;
        private Gamemaster gamemaster;
        private SpellcastingController spellcastingController;
        private CameraDirectionController cameraDirectionController;
        private string lastSpellCast;

        protected override void StartServer()
        {
            spellcastingController = FindObjectOfType<SpellcastingController>();
            if (spellcastingController == null)
            {
                throw new InvalidOperationException($"{nameof(spellcastingController)} is not specified!");
            }
            spellcastingController.SpellCasted += OnSpellCasted;

            gamemaster = GameObject.Find("Gamemaster").GetComponent<Gamemaster>();
            ship = gamemaster.GetShip();
        }

        protected override void StartClient()
        {
            gamemaster = FindObjectsOfType<Gamemaster>().First(gm => gm.gameObject.scene == gameObject.scene);
        }

        private void MoveCamera(Vector3 direction, float intensity)
        {
            if (serverObject.onServer)
            {
                var message = new CameraMovement(direction, intensity);
                SendToAllClients(ByteSerializer.GetBytes(message), Facepunch.Steamworks.Networking.SendType.Reliable);
            }
            else
            {
                var camera = gamemaster.GetCurrentPlayer().GetPlayerCameraController();
                camera.MoveCamera(direction, intensity);
            }
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            var message = ByteSerializer.FromBytes<CameraMovement>(data);
            MoveCamera(message.Direciton, message.i);
        }

        private void OnSpellCasted(object sender, EventArgs args)
        {
            var arguments = (SpellCastedEventArgs)args;
            if (!arguments.Spell.Name.Equals(lastSpellCast))
            {
                Vector3 direction = new Vector3();
                switch (arguments.Spell.Name)
                {
                    case "Tailwind":
                        direction = -ship.transform.forward;
                        break;
                    case "Headwind":
                        direction = ship.transform.forward;
                        break;
                    case "Right halfwind":
                        direction = ship.transform.right;
                        break;
                    case "Left halfwind":
                        direction = -ship.transform.right;
                        break;
                    case "Go UP!":
                        direction = -ship.transform.up;
                        break;
                    case "Down!":
                        direction = ship.transform.up;
                        break;
                }
                lastSpellCast = arguments.Spell.Name;
                MoveCamera(direction, 1.0f);
            }
        }
    }
}
