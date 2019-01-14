using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.PlayerControls {
    public enum PlayerRole
    {
        Spectator = 0,
        Apprentice = 1,
        Wizard = 2

    }

    public static class PlayerRoleExtensions
    {
        public const string LobbyDataKey = "Role";
        const string ActiveRoleKey = "ActivePlayerRole";

        public static string GetUserFriendlyName(this PlayerRole role)
        {
            switch(role)
            {
                case PlayerRole.Wizard: return "Wizard";
                case PlayerRole.Apprentice: return "Apprentice";
                case PlayerRole.Spectator: return "Spectator";
                default: throw new InvalidOperationException($"Unexpected {nameof(PlayerRole)} value of {role}");
            }
        }

        public static ShipBL.InteractablePart.Access GetAccessLevel(this PlayerRole role)
        {
            switch(role)
            {
                case PlayerRole.Wizard: return ShipBL.InteractablePart.Access.Wizard;
                case PlayerRole.Apprentice: return ShipBL.InteractablePart.Access.Apprentice;
                case PlayerRole.Spectator: return ShipBL.InteractablePart.Access.Spectator;
                default: throw new InvalidOperationException($"Unexpected {nameof(PlayerRole)} value of {role}");
            }
        }

        public static void SetPlayerRoleAsActive(this PlayerRole role)
        {
            PlayerPrefs.SetInt(ActiveRoleKey, (int)role);
        }

        public static PlayerRole GetCurrentRole()
        {
            return (PlayerRole)PlayerPrefs.GetInt(ActiveRoleKey, 0);
        }

        public static PlayerInputController AddActiveRoleInputController(GameObject gameObject)
        {
            var role = (PlayerRole)PlayerPrefs.GetInt(ActiveRoleKey);
            switch(role)
            {
                case PlayerRole.Wizard: return gameObject.AddComponent<WizardInput>();
                case PlayerRole.Apprentice: return gameObject.AddComponent<ApprenticeInput>();
                case PlayerRole.Spectator: return gameObject.AddComponent<SpectatorInput>();
                default: throw new InvalidOperationException($"Unexpected {nameof(PlayerRole)} value of {role}");
            }
        }

        public static GameObject GetSpawnPoint(this PlayerRole role)
        {
            switch(role)
            {
                case PlayerRole.Wizard: return GameObject.FindGameObjectWithTag("WizardSpawn");
                case PlayerRole.Apprentice: return GameObject.FindGameObjectWithTag("ApprenticeSpawn");
                case PlayerRole.Spectator: return GameObject.FindGameObjectWithTag("SpectatorSpawn");
                default: throw new InvalidOperationException($"Unexpected {nameof(PlayerRole)} value of {role}");
            }
        }
    }
}
