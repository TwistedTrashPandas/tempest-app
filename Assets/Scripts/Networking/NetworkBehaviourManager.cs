using MastersOfTempest.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MastersOfTempest.Networking
{
    [CreateAssetMenu(fileName = "NetworkBehaviourManager", menuName = "ScriptableObjects/NetworkBehaviourManager")]
    public class NetworkBehaviourManager : ScriptableObject
    {
        // Assign ids to all subclasses of NetworkBehaviour so that there can be multiple NetworkBehaviours on a ServerObject
        public Dictionary<string, int> typeNameToTypeId = new Dictionary<string, int>();
        public string[] typeIdToTypeName;

        [InitializeOnLoadMethod]
        static void CreateNetworkBehaviourTypeIds()
        {
            NetworkBehaviourManager asset = AssetDatabase.LoadAssetAtPath<NetworkBehaviourManager>("Assets/Resources/NetworkBehaviourManager.asset");
            asset.typeNameToTypeId.Clear();
            asset.typeIdToTypeName = null;

            ServerObject[] serverObjects = Resources.LoadAll<ServerObject>("ServerObjects/");

            int idToAssign = 0;

            foreach (ServerObject s in serverObjects)
            {
                NetworkBehaviour[] networkBehaviours = s.GetComponents<NetworkBehaviour>();

                foreach (NetworkBehaviour n in networkBehaviours)
                {
                    string typeName = n.GetType().FullName;

                    if (!asset.typeNameToTypeId.ContainsKey(typeName))
                    {
                        asset.typeNameToTypeId[typeName] = idToAssign;
                        idToAssign++;
                    }
                }
            }

            asset.typeIdToTypeName = new string[asset.typeNameToTypeId.Count];

            foreach (KeyValuePair<string, int> kv in asset.typeNameToTypeId)
            {
                asset.typeIdToTypeName[kv.Value] = kv.Key;
            }

            // Make sure that changes to this asset are saved
            EditorUtility.SetDirty(asset);
        }

        public int GetTypeIdOfNetworkBehaviour(System.Type networkBehaviourType)
        {
            return typeNameToTypeId[networkBehaviourType.FullName];
        }

        public System.Type GetTypeOfNetworkBehaviour(int networkBehaviourTypeId)
        {
            return System.Type.GetType(typeIdToTypeName[networkBehaviourTypeId], true);
        }
    }
}
