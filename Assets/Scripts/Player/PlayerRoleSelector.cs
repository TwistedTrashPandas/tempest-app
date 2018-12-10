using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;

namespace MastersOfTempest.PlayerControls
{
    public class PlayerRoleSelector : MonoBehaviour
    {
        public void SelectWizard ()
        {
            SetRole(PlayerRole.Wizard);
        }

        public void SelectApprentice ()
        {
            SetRole(PlayerRole.Apprentice);
        }

        public void SelectSpectator ()
        {
            SetRole(PlayerRole.Spectator);
        }

        private void SetRole (PlayerRole playerRole)
        {
            PlayerRoleExtensions.SetPlayerRoleAsActive(playerRole);
            Client.Instance.Lobby.SetMemberData("Role", "" + (int)playerRole);
        }
    }
}
