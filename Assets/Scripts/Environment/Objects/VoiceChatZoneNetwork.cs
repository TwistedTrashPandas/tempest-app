
using MastersOfTempest.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace MastersOfTempest.Environment.Interacting
{
    public class VoiceChatZoneNetwork : NetworkBehaviour
    {
        private VoiceChat[] allVoiceChats;
        public AudioMixer audioMixer;
        public Dictionary<VoiceChatZoneType, AudioMixerGroup> audioMixerGroups;

        protected override void StartClient()
        {
            allVoiceChats = FindObjectsOfType<VoiceChat>();
            audioMixerGroups = new Dictionary<VoiceChatZoneType, AudioMixerGroup>();
            AudioMixerGroup[] tempGroups = audioMixer.FindMatchingGroups(string.Empty);
            foreach (VoiceChatZoneType type in Enum.GetValues(typeof(VoiceChatZoneType)))
            {
                AudioMixerGroup toUse = tempGroups[0];
                for (int i = 0; i < tempGroups.Length; i++)
                {
                    if (tempGroups[i].name == type.ToString())
                        toUse = tempGroups[i];
                }
                print(toUse.name);
                audioMixerGroups.Add(type, toUse);
            }
        }

        public void SetVoiceChatZoneType(uint enumerate)
        {
            byte[] data;
            data = ByteSerializer.GetBytes<uint>(enumerate);
            SendToAllClients(data, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            uint enumerate = ByteSerializer.FromBytes<uint>(data);
            for (int i = 0; i < allVoiceChats.Length; i++)
                allVoiceChats[i].setAudioMixerGroup(audioMixerGroups[(VoiceChatZoneType) enumerate]);
        }
    }
}