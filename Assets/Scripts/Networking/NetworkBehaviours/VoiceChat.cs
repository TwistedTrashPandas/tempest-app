using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MastersOfTempest.Networking
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceChat : NetworkBehaviour
    {
        public bool hearYourself = false;

        private AudioSource audioSource;

        protected override void StartClient()
        {
            audioSource = GetComponent<AudioSource>();

            Facepunch.Steamworks.Client.Instance.Voice.OnCompressedData += OnCompressedData;
            Facepunch.Steamworks.Client.Instance.Voice.WantsRecording = true;
        }

        private void OnCompressedData(byte[] data, int dataLength)
        {
            byte[] compressedData = new byte[dataLength];
            Array.Copy(data, compressedData, dataLength);

            // Send to all clients but yourself (if you don't want to hear yourself)
            ulong[] memberIDs = NetworkManager.Instance.GetLobbyMemberIDs();

            foreach (ulong steamID in memberIDs)
            {
                if (hearYourself || steamID != Facepunch.Steamworks.Client.Instance.SteamId)
                {
                    SendToClient(steamID, compressedData, Facepunch.Steamworks.Networking.SendType.Reliable);
                }
            }
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();

            if (Facepunch.Steamworks.Client.Instance.Voice.Decompress(data, stream))
            {
                // 16 bit signed PCM data
                byte[] uncompressedData = stream.ToArray();

                float[] samples = new float[uncompressedData.Length / 2];

                for (int i = 0; i < uncompressedData.Length; i += 2)
                {
                    samples[i / 2] = (BitConverter.ToInt16(uncompressedData, i) / (float)Int16.MaxValue);
                }

                // TODO: Use streaming feature instead of this
                AudioClip clip = AudioClip.Create("Voice Chat", samples.Length, 1, (int)Facepunch.Steamworks.Client.Instance.Voice.OptimalSampleRate, false);
                clip.SetData(samples, 0);
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogError("Failed to decompress Voice Chat data!");
            }
        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            SendToAllClients(data, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnDestroyClient()
        {
            if (Facepunch.Steamworks.Client.Instance != null)
            {
                Facepunch.Steamworks.Client.Instance.Voice.OnCompressedData -= OnCompressedData;
            }
        }
    }
}
