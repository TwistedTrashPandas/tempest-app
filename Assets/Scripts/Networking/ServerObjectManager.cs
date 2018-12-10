using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MastersOfTempest.Networking;

public class ServerObjectManager
{
    [InitializeOnLoadMethod]
    static void AssignServerObjectResourceIDs()
    {
        // Assign a resource id to each server object but make sure not to change already assigned ids
        ServerObject[] serverObjectResources = Resources.LoadAll<ServerObject>("ServerObjects/");

        Dictionary<int, ServerObject> alreadyAssignedResourceIDs = new Dictionary<int, ServerObject>();

        foreach (ServerObject s in serverObjectResources)
        {
            if (s.resourceID >= 0 && !alreadyAssignedResourceIDs.ContainsKey(s.resourceID))
            {
                // Valid id, add it to the set
                alreadyAssignedResourceIDs[s.resourceID] = s;
            }
            else
            {
                // Duplicated id or no id set
                s.resourceID = -1;
            }
        }

        int nextIdToAssign = 0;

        foreach (ServerObject s in serverObjectResources)
        {
            while (alreadyAssignedResourceIDs.ContainsKey(nextIdToAssign))
            {
                nextIdToAssign++;
            }

            if (s.resourceID == -1)
            {
                s.resourceID = nextIdToAssign;
                EditorUtility.SetDirty(s);
                nextIdToAssign++;
            }
        }
    }
}