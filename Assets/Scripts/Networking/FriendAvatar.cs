﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MastersOfTempest.Networking
{
    public class FriendAvatar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ulong steamID;
        public bool ready = false;
        public PlayerControls.PlayerRole role = PlayerControls.PlayerRole.Spectator;

        public Image avatar;
        public Image imageNameBackground;
        public Image imageReadyOutline;
        public Button buttonInvite;
        public Text textRole;
        public Text textName;

        void Start()
        {
            textName.text = gameObject.name;
        }

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

        public void Invite()
        {
            Facepunch.Steamworks.Client.Instance.Lobby.InviteUserToLobby(steamID);
        }

        public void OnImage(Facepunch.Steamworks.Image image)
        {
            if (image.IsError)
            {
                //Debug.Log("Failed to load avatar of user " + steamID + ". Trying again...");
                Facepunch.Steamworks.Client.Instance.Friends.GetAvatar(Facepunch.Steamworks.Friends.AvatarSize.Large, steamID, OnImage);
            }
            else
            {
                Texture2D texture = new Texture2D(image.Width, image.Height);

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Facepunch.Steamworks.Color c = image.GetPixel(x, y);
                        texture.SetPixel(x, image.Height - y, new Color(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, c.a / 255.0f));
                    }
                }

                texture.Apply();

                avatar.sprite = Sprite.Create(texture, new Rect(0, 0, image.Width, image.Height), Vector2.zero);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            imageNameBackground.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            imageNameBackground.gameObject.SetActive(false);
        }
    }
}
