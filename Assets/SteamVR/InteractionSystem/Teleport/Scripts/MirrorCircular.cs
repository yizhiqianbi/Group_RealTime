using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MirrorCircular : MirrorBasicFormation
{
    public float radius = 1.5f;
    public float minRadius = 1f;
    public float maxRadius = 2.5f;
    private float angle;
    public override void MakeFormation()
    {
        int member_num = preAvatars.Count;
        lines = new GameObject[member_num];
        angle = 360f / member_num;
        for (int i = 0; i < member_num; i++)
        {
            //设置预览形象坐标
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * i * angle);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * i * angle);
            preAvatars[i].transform.localPosition = new Vector3(x, offset, z);
        }
        for (int i = 0; i < member_num; i++)
        {
            //连线
            GameObject line = Instantiate(LinePrefeb);
            NetworkServer.Spawn(line);
            line.transform.name = "line_" + i;
            line.transform.parent = preAvatars[i].transform;
            line.GetComponent<LineScript>().SetStartPosition(preAvatars[i].transform.position - new Vector3(0, offset, 0));
            line.GetComponent<LineScript>().SetEndPosition(preAvatars[(i + 1) % member_num].transform.position - new Vector3(0, offset, 0));
            lines[i] = line;
        }
    }

    public override void Largen()
    {
        if (radius >= maxRadius)
        {
            return;
        }
        radius += (maxRadius - minRadius) * 0.0001f * 100;
        UpdateScale();
    }

    public override void Lessen()
    {
        if (radius <= minRadius)
        {
            return;
        }
        radius -= (maxRadius - minRadius) * 0.0001f * 100;
        UpdateScale();
    }

    private void UpdateScale()
    {
        int member_num = preAvatars.Count;
        for (int i = 0; i < member_num; i++)
        {
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * i * angle);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * i * angle);
            preAvatars[i].transform.localPosition = new Vector3(x, offset, z);
        }
    }

    public new void Awake()
    {
        base.Awake();
        formationType = "circle";
    }
}
