using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepVertical : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 headForward = transform.parent.parent.forward;
        transform.forward = -new Vector3(headForward.x, 0, headForward.z);
    }
}
