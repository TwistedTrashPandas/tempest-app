using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MastersOfTempest.Environment.Interacting
{
    public class VoiceChatZone : EnvObject
    {
        public VoiceChatZoneType zoneType;
        public AudioMixer audioMixer;
        public Dictionary<VoiceChatZoneType, AudioMixerGroup> audioMixerGroups;

        public void Initialize()
        {
            audioMixerGroups = new Dictionary<VoiceChatZoneType, AudioMixerGroup>();
            AudioMixerGroup[] tempGroups = audioMixer.FindMatchingGroups(string.Empty);
            foreach (VoiceChatZoneType type in Enum.GetValues(typeof(VoiceChatZoneType)))
            {
                if (type == VoiceChatZoneType.Normalized || type == zoneType)
                {
                    AudioMixerGroup toUse = tempGroups[0];
                    for (int i = 0; i < tempGroups.Length; i++)
                    {
                        if (tempGroups[i].name == type.ToString())
                            toUse = tempGroups[i];
                    }
                    if (toUse != null)
                        print(toUse.name);
                    audioMixerGroups.Add(type, toUse);
                }
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "ship")
                audioMixer.outputAudioMixerGroup = audioMixerGroups[zoneType];
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "ship")
                audioMixer.outputAudioMixerGroup = audioMixerGroups[VoiceChatZoneType.Normalized];
        }
    }
}
