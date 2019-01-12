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
        private VoiceChatZoneNetwork voiceChatZoneNetwork;

        private void Start()
        {
            voiceChatZoneNetwork = GameObject.FindObjectOfType<VoiceChatZoneNetwork>();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Ship")
            {
                voiceChatZoneNetwork.SetVoiceChatZoneType((uint)zoneType);
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            Debug.Log(other.gameObject.tag);
            if (other.gameObject.tag == "Ship")
            {
                voiceChatZoneNetwork.SetVoiceChatZoneType((uint)VoiceChatZoneType.Normalized);
            }
        }
    }
}
