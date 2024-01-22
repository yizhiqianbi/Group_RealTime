using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Valve.VR;

public class MirrorGrid : MirrorBasicFormation
{
    int size;
    public float distance = 1;
    public float minDistance = 0.5f;
    public float maxDistance = 2f;
    int[] boundPoints;
    public override void MakeFormation()
    {
        int member_num = preAvatars.Count;
        size = Mathf.CeilToInt(Mathf.Sqrt(member_num));
        if(member_num == 5)
        {
            boundPoints = new int[5] { 0, 1, 2, 4, 3 };
        }else if(member_num == 10)
        {
            boundPoints = new int[8] { 0, 1, 2, 3, 7, 9, 8, 4 };
        }else if(member_num == 15)
        {
            boundPoints = new int[11] { 0, 1, 2, 3, 7, 11, 14, 13, 12, 8, 4 };
        }
        else
        {
            Debug.Log("MirrorGrid遇到了member_num不为5、10、15的情况");
        }
        lines = new GameObject[boundPoints.Length];
        float l = distance * (size - 1);
        float left_x = -l / 2;
        float down_z = -l / 2;
        for(int i = 0; i < member_num; i++)
        {
            int line = i / size;
            int col = i % size;
            float x = left_x + col * distance;
            float z = down_z + line * distance;
            preAvatars[i].transform.localPosition = new Vector3(x, offset, z);
        }

        for(int i = 0; i < boundPoints.Length; i++)
        {
            GameObject line = Instantiate(LinePrefeb);
            NetworkServer.Spawn(line);
            int index = boundPoints[i];
            int nextIndex = boundPoints[(i + 1) % boundPoints.Length];
            line.transform.name = "line_" + index;
            line.transform.parent = preAvatars[index].transform;
            line.GetComponent<LineScript>().SetStartPosition(preAvatars[index].transform.position - new Vector3(0, offset, 0));
            line.GetComponent<LineScript>().SetEndPosition(preAvatars[nextIndex].transform.position - new Vector3(0, offset, 0));
            lines[i] = line;
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
        float l = distance * (size - 1);
        float left_x = -l / 2;
        float down_z = -l / 2;
        for (int i = 0; i < member_num; i++)
        {
            int line = i / size;
            int col = i % size;
            float x = left_x + col * distance;
            float z = down_z + line * distance;
            preAvatars[i].transform.localPosition = new Vector3(x, offset, z);
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
        Vector3 line = preAvatars[0].transform.position - preAvatars[size].transform.position;
        Vector3 forward = new Vector3(line.x, 0, line.z);//朝向前方
        for (int i = 0; i < member_num; i++)
        {
            preAvatars[i].transform.forward = forward;
        }
        for (int i = 0; i < boundPoints.Length; i++)
        {
            int index = boundPoints[i];
            int nextIndex = boundPoints[(i + 1) % boundPoints.Length];
            lines[i].GetComponent<LineScript>().SetStartPosition(preAvatars[index].transform.position - new Vector3(0, offset, 0));
            lines[i].GetComponent<LineScript>().SetEndPosition(preAvatars[nextIndex].transform.position - new Vector3(0, offset, 0));
        }
    }

    public override void CheckValidation()
    {
        foreach (GameObject line in lines)
        {
            line.GetComponent<LineScript>().SetLineColor(Color.black);
        }
        for(int i = 0; i < boundPoints.Length; i++)
        {
            bool isValid = false;
            int index = boundPoints[i];
            Vector3 preAvatarPosition = preAvatars[index].transform.position;
            Collider[] colliders = Physics.OverlapSphere(new Vector3(preAvatarPosition.x, preAvatarPosition.y - offset - 0.1f, preAvatarPosition.z), 0.05f);
            if (colliders.Length != 0)
            {
                isValid = true;
            }
            if (!isValid)
            {
                lines[i].GetComponent<LineScript>().SetLineColor(Color.red);
                lines[(i - 1 + boundPoints.Length) % boundPoints.Length].GetComponent<LineScript>().SetLineColor(Color.red);
            }
        }
    }

    public new void Awake()
    {
        base.Awake();
        formationType = "grid";
    }
}
