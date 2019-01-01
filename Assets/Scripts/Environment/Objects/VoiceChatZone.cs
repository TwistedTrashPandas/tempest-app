using MastersOfTempest.Networking;
using MastersOfTempest.ShipBL;
using System.Collections;
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
            AudioMixerGroup[] tempGroups = audioMixer.FindMatchingGroups(string.Empty);
            print(tempGroups.Length);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "ship")
                audioMixer.outputAudioMixerGroup = audioMixerGroups[zoneType];
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "ship")
                audioMixer.outputAudioMixerGroup = audioMixerGroups[VoiceChatZoneType.Normalized];
        }
    }
}
