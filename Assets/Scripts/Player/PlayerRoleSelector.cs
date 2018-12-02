using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MastersOfTempest.PlayerControls
{
    [RequireComponent(typeof(UnityEngine.UI.Dropdown))]
    public class PlayerRoleSelector : MonoBehaviour
    {
        private Dropdown dropdown;

        private void Awake()
        {
            dropdown = GetComponent<Dropdown>();
            
            var options = new List<string>();
            foreach(PlayerRole role in Enum.GetValues(typeof(PlayerRole)))
            {
                options.Insert((int) role, role.GetUserFriendlyName());
            }
            dropdown.AddOptions(options);
        }

        private void OnEnable()
        {
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void Start()
        {
            dropdown.value = (int) PlayerRoleExtensions.GetCurrentRole();
        }

        private void OnDropdownChanged(int newValue)
        {
            ((PlayerRole) newValue).SetPlayerRoleAsActive();
        }
    }
}
