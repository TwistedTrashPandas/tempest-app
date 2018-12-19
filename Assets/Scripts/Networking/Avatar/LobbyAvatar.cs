using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MastersOfTempest.Networking
{
    public class LobbyAvatar : Avatar
    {
        public bool ready = false;
        public PlayerControls.PlayerRole role = PlayerControls.PlayerRole.Spectator;

        public Image imageReadyOutline;
        public Text textRole;

        public void Refresh ()
        {
            if (bool.TryParse(Facepunch.Steamworks.Client.Instance.Lobby.GetMemberData(steamID, "Ready"), out ready))
            {
                imageReadyOutline.color = ready ? Color.green : Color.red;
            }

            int roleKey;
            if (int.TryParse(Facepunch.Steamworks.Client.Instance.Lobby.GetMemberData(steamID, "Role"), out roleKey))
            {
                role = (PlayerControls.PlayerRole)roleKey;
                textRole.text = PlayerControls.PlayerRoleExtensions.GetUserFriendlyName(role);
            }
        }
    }
}
