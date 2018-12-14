using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    public class EditorNetworkBehaviourManager
    {
        [InitializeOnLoadMethod]
        static void CreateNetworkBehaviourTypeIds()
        {
            NetworkBehaviourTypeContainer asset = AssetDatabase.LoadAssetAtPath<NetworkBehaviourTypeContainer>("Assets/Resources/NetworkBehaviourTypeContainer.asset");
            asset.networkBehaviourTypeFullNames = null;

            ServerObject[] serverObjects = Resources.LoadAll<ServerObject>("ServerObjects/");

            HashSet<string> setOfTypeFullNames = new HashSet<string>();

            foreach (ServerObject s in serverObjects)
            {
                NetworkBehaviour[] networkBehaviours = s.GetComponents<NetworkBehaviour>();

                foreach (NetworkBehaviour n in networkBehaviours)
                {
                    string typeName = n.GetType().FullName;

                    if (!setOfTypeFullNames.Contains(typeName))
                    {
                        setOfTypeFullNames.Add(typeName);
                    }
                }
            }

            int idToAssign = 0;
            asset.networkBehaviourTypeFullNames = new string[setOfTypeFullNames.Count];

            foreach (string typeFullName in setOfTypeFullNames)
            {
                asset.networkBehaviourTypeFullNames[idToAssign] = typeFullName;
                idToAssign++;
            }

            // Make sure that changes to this asset are saved
            EditorUtility.SetDirty(asset);
        }
    }
}
