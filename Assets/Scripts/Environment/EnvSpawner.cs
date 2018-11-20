using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvSpawner : MonoBehaviour {

    public GameObject prefab;
    public VectorField vectorField;

    private List<EnvObject> envObjects;

	// Use this for initialization
	void Start () {
        envObjects = new List<EnvObject>();
        InstantiateNewObject();
    }

    private void Update()
    {
        for(int i = 0; i< envObjects.Count; i++)
        {
            envObjects[i].SetVelocity(vectorField.GetVectorAtPos(envObjects[i].transform.position));
        }
    }

    private void InstantiateNewObject()
    {
        envObjects.Add(GameObject.Instantiate(prefab).GetComponent<EnvObject>());
    }
}
