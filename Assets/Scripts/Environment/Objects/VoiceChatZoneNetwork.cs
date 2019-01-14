
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
                audioMixerGroups.Add(type, toUse);
            }
        }

        public void SetVoiceChatZoneType(int enumerate)
        {
            byte[] data;
            data = ByteSerializer.GetBytes<int>(enumerate);
            SendToAllClients(data, Facepunch.Steamworks.Networking.SendType.Reliable);
        }

        protected override void OnClientReceivedMessageRaw(byte[] data, ulong steamID)
        {
            int enumerate = ByteSerializer.FromBytes<int>(data);
            for (int i = 0; i < allVoiceChats.Length; i++)
            {
                if(!allVoiceChats[i].GetComponent<ServerObject>().onServer)
                    allVoiceChats[i].setAudioMixerGroup(audioMixerGroups[(VoiceChatZoneType)enumerate]);
            }
        }
    }
}