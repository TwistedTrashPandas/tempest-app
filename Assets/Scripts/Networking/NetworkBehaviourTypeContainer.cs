using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    [CreateAssetMenu(fileName = "NetworkBehaviourTypeContainer", menuName = "ScriptableObjects/NetworkBehaviourTypeContainer")]
    public class NetworkBehaviourTypeContainer : ScriptableObject
    {
        // Assign ids to all subclasses of NetworkBehaviour so that there can be multiple NetworkBehaviours on a ServerObject
        public string[] networkBehaviourTypeFullNames;

        public int GetTypeIdOfNetworkBehaviour(System.Type networkBehaviourType)
        {
            // Search for the id instead of using a dictionairy because the dictionairy is not saved for the build
            for (int i = 0; i < networkBehaviourTypeFullNames.Length; i++)
            {
                if (networkBehaviourType.FullName.Equals(networkBehaviourTypeFullNames[i]))
                {
                    return i;
                }
            }

            // No id found
            return -1;
        }

        public System.Type GetTypeOfNetworkBehaviour(int networkBehaviourTypeId)
        {
            return System.Type.GetType(networkBehaviourTypeFullNames[networkBehaviourTypeId], true);
        }
    }
}
