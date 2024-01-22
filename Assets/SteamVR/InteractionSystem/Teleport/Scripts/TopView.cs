using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopView : MonoBehaviour
{
    public Transform desitination;
    private Camera topviewCamera;
    // Start is called before the first frame update
    void Start()
    {
        topviewCamera = GetComponent<Camera>();
    }

    void Update()
    {
        topviewCamera.transform.position = new Vector3(desitination.position.x, topviewCamera.transform.position.y, desitination.position.z);
    }
}
