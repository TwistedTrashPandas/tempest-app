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
        [Header("Hold or press fast twice to toggle recording on/off")]
        public KeyCode recordKey = KeyCode.Tab;
        public Texture recordIcon;
        public bool recording = false;
        public bool mirror = false;

        private AudioSource audioSource;
        private bool toggleRecording = false;
        private float lastTimeKeyDown = -1;

        protected override void StartClient()
        {
            audioSource = GetComponent<AudioSource>();

            Facepunch.Steamworks.Client.Instance.Voice.OnCompressedData += OnCompressedData;
        }

        protected override void UpdateClient()
        {
            if (Input.GetKey(recordKey))
            {
                Facepunch.Steamworks.Client.Instance.Voice.WantsRecording = true;
            }
            else
            {
                Facepunch.Steamworks.Client.Instance.Voice.WantsRecording = toggleRecording;
            }

            if (Input.GetKeyDown(recordKey))
            {
                if ((Time.time - lastTimeKeyDown) < 0.5f)
                {
                    toggleRecording = !toggleRecording;
                }

                lastTimeKeyDown = Time.time;
            }

            recording = Facepunch.Steamworks.Client.Instance.Voice.IsRecording;
        }

        private void OnCompressedData(byte[] data, int dataLength)
        {
            byte[] compressedData = new byte[dataLength];
            Array.Copy(data, compressedData, dataLength);

            // Send to all clients except yourself (if you don't want to mirror yourself)
            ulong[] memberIDs = NetworkManager.Instance.GetLobbyMemberIDs();

            foreach (ulong steamID in memberIDs)
            {
                if (mirror || steamID != Facepunch.Steamworks.Client.Instance.SteamId)
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

                if (samples.Length > 0)
                {
                    // Create a new clip and play it (should be able to play multiple user voices at the same time)
                    // Maybe this can be improved by also taking into account the time between recordings
                    AudioClip clip = AudioClip.Create("Voice", samples.Length, 1, (int)Facepunch.Steamworks.Client.Instance.Voice.OptimalSampleRate, false);
                    clip.SetData(samples, 0);
                    audioSource.PlayOneShot(clip);
                }
            }
            else
            {
                Debug.LogWarning("Failed to decompress voice chat data.");
            }
        }

        private void OnGUI()
        {
            if (!serverObject.onServer && recording)
            {
                GUI.Label(new Rect(Screen.width / 4, 2 * (Screen.height / 3), 100, 100), recordIcon);
            }
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
