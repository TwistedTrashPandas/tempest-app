using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendAvatar : MonoBehaviour
{
    public ulong steamID;

    public Image image;
    public Button buttonInvite;

    public void Invite ()
    {
        Facepunch.Steamworks.Client.Instance.Lobby.InviteUserToLobby(steamID);
    }

    public void OnImage(Facepunch.Steamworks.Image image)
    {
        if (!image.IsError)
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

            this.image.sprite = Sprite.Create(texture, new Rect(0, 0, image.Width, image.Height), Vector2.zero);
        }
    }
}
