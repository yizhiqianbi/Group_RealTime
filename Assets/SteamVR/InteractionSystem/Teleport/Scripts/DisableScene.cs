using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableScene : MonoBehaviour
{
    public GameObject[] objs;
    public bool enable;
    private bool oldEnable;

    public void Awake()
    {
        enable = oldEnable = true;
    }

    public void Update()
    {
        if (enable == oldEnable) return;
        oldEnable = enable;
        foreach(GameObject obj in objs)
        {
            obj.SetActive(enable);
        }
    }

}
