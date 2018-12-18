﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MastersOfTempest.Networking
{
    public class EditorServerObjectManager
    {
        [InitializeOnLoadMethod]
        static void AssignServerObjectResourceIDs()
        {
            // Assign a resource id to each server object but make sure not to change already assigned ids
            ServerObject[] serverObjectResources = Resources.LoadAll<ServerObject>("ServerObjects/");

            Dictionary<int, ServerObject> alreadyAssignedResourceIDs = new Dictionary<int, ServerObject>();

            foreach (ServerObject s in serverObjectResources)
            {
                // Assign resource id
                if (s.resourceID > 0 && !alreadyAssignedResourceIDs.ContainsKey(s.resourceID))
                {
                    // Valid id, add it to the set
                    alreadyAssignedResourceIDs[s.resourceID] = s;
                }
                else
                {
                    // Duplicated id or no id set
                    s.resourceID = 0;
                }

                // Assign children and resource id as -(1 + childId)
                List<ServerObject> children = new List<ServerObject>();
                s.GetComponentsInChildren(true, children);
                children.Remove(s);

                s.children = children.ToArray();

                for (int i = 0; i < s.children.Length; i++)
                {
                    ServerObject child = s.children[i];
                    int resourceIdAsChildId = -(1 + i);

                    if (child.resourceID != resourceIdAsChildId || child.root != s)
                    {
                        // Assign resource id / root and save changes
                        s.children[i].resourceID = resourceIdAsChildId;
                        s.children[i].root = s;
                        EditorUtility.SetDirty(s);
                    }
                }

                if (s.root != s)
                {
                    // Assign root to itself and save changes
                    s.root = s;
                    EditorUtility.SetDirty(s);
                }
            }

            int nextIdToAssign = 1;

            foreach (ServerObject s in serverObjectResources)
            {
                while (alreadyAssignedResourceIDs.ContainsKey(nextIdToAssign))
                {
                    nextIdToAssign++;
                }

                if (s.resourceID == 0)
                {
                    // Assign id and make sure that changes to this prefab are saved
                    s.resourceID = nextIdToAssign;
                    EditorUtility.SetDirty(s);
                    nextIdToAssign++;
                }
            }
        }
    }
}