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
    public float force = 3f;
    public float rotSpeed = 2.5f;
    private Vector3[] randomDir = new Vector3[5] ;
    private Quaternion[] randomRot = new Quaternion[5];
    
    void Start()
    {
        parent = gameObject.GetComponentInParent<Transform>().gameObject;

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

       
        children.ToArray();
    }

    
    void Update()
    {
        if(Input.GetKeyDown("k"))
        {
            InitValues();
            isDestroyed = true;
        }
        if(isDestroyed == true)
        {
            SplitRock();
        }
    }

    private void InitValues()
    {
        Debug.Log("initValue");
        foreach(GameObject a in children)
        {
            a.transform.parent = null;
        }
       
        children.Add(parent);

        Debug.Log(children.Count);
        Debug.Log(randomDir.Length);

        randomDir[0] = new Vector3(Random.value, Random.value, Random.value);
        randomDir[1] = new Vector3(randomDir[0].x * (-1.0f), randomDir[0].y * (-1.0f), randomDir[0].z * (-1.0f)) ;
        randomDir[2] = new Vector3(randomDir[1].z, randomDir[1].y, randomDir[1].x);

        randomRot[0] = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        randomRot[1] = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));
        randomRot[2] = Quaternion.Euler(new Vector3(Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f), Random.Range(-180.0f, 180.0f)));


        Debug.Log(randomDir.Length);
        init = true;
    }

    private void SplitRock()
    {
        Debug.Log("I'm here!");
        Debug.Log(children[0].name);
        Debug.Log(children[1].name);
        int j = 0;
        foreach (GameObject a in children)
        {
            a.transform.Translate(randomDir[j] * Time.deltaTime * force);
            // a.gameObject.transform.rotation = Quaternion.Slerp(transform.rotation, randomRot[j], Time.deltaTime * rotSpeed);
            a.transform.Rotate(randomRot[j].eulerAngles*Time.deltaTime*rotSpeed);
            j++;
        }
       
    }
}
