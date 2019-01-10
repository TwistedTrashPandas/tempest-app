using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//On collsision detection the rocks shall split and fly off in different directions. 

public class RockAnimator : MonoBehaviour
{
    public GameObject parent;
    public List<GameObject> children;

    public bool isDestroyed = false;
    public bool init = false;
    public float force = 5.0f;
    public float rotSpeed = 0.3f;
    private Vector3[] randomDir = new Vector3[5] ;
    private Quaternion[] randomRot = new Quaternion[5];
    
    void Start()
    {
        /*Already done in Damaging.cs
        parent = gameObject.GetComponentInParent<Transform>().gameObject;

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

       
        children.ToArray();
        */

        InitValues();
    }

    
    void Update()
    {
            SplitRock();    
    }

    private void InitValues()
    {
        /* Already done in Damaging.cs
        Debug.Log("initValue");
        foreach(GameObject a in children)
        {
            a.transform.parent = null;
        }
       
        children.Add(parent);
        */

        randomDir[0] = new Vector3(Random.value, Random.value, Random.value);
        randomDir[1] = new Vector3(randomDir[0].x * (-1.0f), randomDir[0].y * (-1.0f), randomDir[0].z * (-1.0f)) ;
        randomDir[2] = new Vector3(randomDir[1].z, randomDir[1].y, randomDir[1].x);

        randomRot[0] = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        randomRot[1] = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        randomRot[2] = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));

        init = true;
    }

    private void SplitRock()
    {
        int j = 0;
        foreach (GameObject a in children)
        {
            a.transform.Translate(randomDir[j] * Time.deltaTime * force);
            a.transform.Rotate(randomRot[j].eulerAngles*Time.deltaTime*rotSpeed);
            j++;
        }
       
    }
}
