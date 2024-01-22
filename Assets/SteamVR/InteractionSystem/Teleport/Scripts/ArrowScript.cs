using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    public void SetArrowColor(Color color)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    public void SetArrowForward(Vector3 forward)
    {
        transform.forward = new Vector3(forward.x, 0, forward.z);
    }
}
