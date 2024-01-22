using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> vector3s = new List<Vector3> { Vector3.zero, Vector3.one };
        Vector3[] vector3s1 = new Vector3[2];
        vector3s1[1] = vector3s[0];
        vector3s1[0] = vector3s[1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
