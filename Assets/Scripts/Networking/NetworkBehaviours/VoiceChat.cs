using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facepunch.Steamworks;
using System;

namespace MastersOfTempest.Networking
{
    public class VoiceChat : NetworkBehaviour
    {
        protected override void StartClient()
        {
            Client.Instance.Voice.OnCompressedData += OnCompressedData;
            Client.Instance.Voice.WantsRecording = true;
        }

        private void OnCompressedData(byte[] data, int dataLength)
        {
            byte[] realData = new byte[dataLength];
            Array.Copy(data, realData, dataLength);

            // Error because the size is too big for UDP messages
            //SendToServer(data, Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {

        }

        protected override void OnServerReceivedMessageRaw(byte[] data, ulong steamID)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            Client.Instance.Voice.Decompress(data, stream);

            float[] samples = new float[data.Length / 4];

            for (int i = 0; i < data.Length; i++)
            {
                samples[i] = BitConverter.ToSingle(data, i);
            }

            AudioClip clip = AudioClip.Create("Voice Chat", samples.Length, 1, (int)Client.Instance.Voice.OptimalSampleRate, false);
            clip.SetData(samples, 0);

            FindObjectOfType<AudioSource>().PlayOneShot(clip);
        }
    }
}
