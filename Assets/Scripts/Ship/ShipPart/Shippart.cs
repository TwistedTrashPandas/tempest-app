using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shippart : MonoBehaviour
{
    public int interactionArea;
    public float cutOffDist = 20f;
    public float impulseScaling = 0.5f;

    /// <summary>
    /// destruction == 0:   ship part fully repaired
    ///             == 1:   ship part fully destroyed
    /// </summary>
    private float destruction;
    private Vector3[] initialMesh;
    private Vector3[] targetMesh;


    private void Start()
    {
        initialMesh = GetComponent<MeshFilter>().mesh.vertices;
        targetMesh = initialMesh;
        if (initialMesh == null)
            throw new System.InvalidOperationException("Ship part can only be attached to objects with meshes");
    }

    private void Update()
    {
        InterpolateCurrentMesh();
    }

    public float GetDestruction()
    {
        return destruction;
    }

    public void ResolveCollision(float destruc, ContactPoint[] contactPoints, Vector3 impulse)
    {
        UpdateMesh(contactPoints, impulse);
        AddDestruction(destruc);
        //InterpolateCurrentMesh();
    }

    public void UpdateMesh(ContactPoint[] contactPoints, Vector3 impulse)
    {
        Vector3[] currVerts = GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            Vector3 currContact = contactPoints[i].point;
            //Vector3 normal = contactPoints[i].normal;
            for (int j = 0; j < currVerts.Length; j++)
            {
                Vector3 worldPos = transform.TransformPoint(currVerts[j]);
                float distSquared = Vector3.Distance(worldPos, currContact);
                if (cutOffDist > distSquared)
                {
                    Vector3 dir = (worldPos - currContact).normalized;
                    if (Vector3.Dot(dir, impulse.normalized) > 0)
                        dir *= impulse.magnitude;
                    else
                        dir = impulse;
                    distSquared *= distSquared;
                    worldPos += dir / (distSquared + 1f) / contactPoints.Length * impulseScaling;
                }
                currVerts[j] = transform.InverseTransformPoint(worldPos);
            }
        }
        GetComponent<MeshFilter>().mesh.vertices = currVerts;
        targetMesh = currVerts;
    }

    // add or remove destruction value to ship parts
    public void AddDestruction(float destruc)
    { 
        destruction = Mathf.Clamp01(destruction + destruc);
        if(destruc < 0)
            InterpolateCurrentMesh();
    }


    private void InterpolateCurrentMesh()
    {
        Mesh currM = GetComponent<MeshFilter>().mesh;
        Vector3[] vert = currM.vertices;
        for (int i = 0; i < vert.Length; i++)
            vert[i] = Vector3.Lerp(initialMesh[i], targetMesh[i], destruction);
        GetComponent<MeshFilter>().mesh.vertices = vert;
    }
}
