using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Valve.VR;

public class MirrorLinear : MirrorBasicFormation
{
    public float distance = 1;
    public float minDistance = 0.5f;
    public float maxDistance = 2f;

    public override void MakeFormation()
    {
        int member_num = preAvatars.Count;
        lines = new GameObject[member_num - 1];
        float left_x = -(member_num - 1) * distance / 2;
        for (int i = 0; i < member_num; i++)
        {
            preAvatars[i].transform.localPosition = new Vector3(left_x + distance * i, offset, 0);
        }
        for (int i = 0; i < member_num - 1; i++)
        {
            GameObject line = Instantiate(LinePrefeb);
            NetworkServer.Spawn(line);
            line.transform.name = "line_" + i;
            line.transform.parent = preAvatars[i].transform;
            line.GetComponent<LineScript>().SetStartPosition(preAvatars[i].transform.position - new Vector3(0, offset, 0));
            line.GetComponent<LineScript>().SetEndPosition(preAvatars[(i + 1) % member_num].transform.position - new Vector3(0, offset, 0));
            lines[i] = line;
        }
    }

    public override void UpdateFormation()
    {
        bool rotateLeft = rotateLeftAction.GetState(SteamVR_Input_Sources.LeftHand);
        bool rotateRight = rotateRightAction.GetState(SteamVR_Input_Sources.LeftHand);
        bool largen = largenAction.GetState(SteamVR_Input_Sources.LeftHand);
        bool lessen = lessenAction.GetState(SteamVR_Input_Sources.LeftHand);
        if (largen)
        {
            Largen();
        }
        else if (lessen)
        {
            Lessen();
        }
        if (rotateLeft)
        {
            RotateLeft();
        }
        else if (rotateRight)
        {
            RotateRight();
        }

        //更新朝向和线条
        int member_num = preAvatars.Count;
/*        Vector3 line = preAvatars[1].transform.position - preAvatars[0].transform.position;
        Vector3 forward = new Vector3(-line.z, 0, line.x);//法向量，方向垂直于直线*/
        Vector3 line = preAvatars[0].transform.position - preAvatars[1].transform.position;
        Vector3 forward = new Vector3(line.x, 0, line.z);//朝向导游
        for (int i = 0; i < member_num; i++)
        {
            preAvatars[i].transform.forward = forward;
            if (i != member_num - 1)
            {
                lines[i].GetComponent<LineScript>().SetStartPosition(preAvatars[i].transform.position - new Vector3(0, offset, 0));
                lines[i].GetComponent<LineScript>().SetEndPosition(preAvatars[(i + 1) % member_num].transform.position - new Vector3(0, offset, 0));
            }
        }
    }

    public override void Largen()
    {
        if (distance > maxDistance)
        {
            return;
        }
        distance += (maxDistance - minDistance) * 0.0001f * 100;
        UpdateScale();
    }

    public override void Lessen()
    {
        if (distance < minDistance)
        {
            return;
        }
        distance -= (maxDistance - minDistance) * 0.0001f * 100;
        UpdateScale();
    }

    void UpdateScale()
    {
        int member_num = preAvatars.Count;
        float left_x = -(member_num - 1) * distance / 2;
        for (int i = 0; i < member_num; i++)
        {
            preAvatars[i].transform.localPosition = new Vector3(left_x + distance * i, offset, 0);
        }
    }

    public override void CheckValidation()
    {
        foreach (GameObject line in lines)
        {
            line.GetComponent<LineScript>().SetLineColor(Color.black);
        }
        int member_num = preAvatars.Count;
        for (int i = 0; i < member_num; i++)
        {
            bool isValid = false;
            Vector3 preAvatarPosition = preAvatars[i].transform.position;
            Collider[] colliders = Physics.OverlapSphere(new Vector3(preAvatarPosition.x, preAvatarPosition.y - offset - 0.1f, preAvatarPosition.z), 0.05f);
            if (colliders.Length != 0)
            {
                isValid = true;
            }
            if (!isValid)
            {
                if (i < member_num - 1)
                {
                    lines[i].GetComponent<LineScript>().SetLineColor(Color.red);
                }
                if (i > 0)
                {
                    lines[(i - 1 + member_num) % member_num].GetComponent<LineScript>().SetLineColor(Color.red);
                }
            }
        }
    }

    public new void Awake()
    {
        base.Awake();
        formationType = "queue";
    }
}
