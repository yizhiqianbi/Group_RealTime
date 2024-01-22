using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;

[Serializable]
public class CurlingthrowData
{
    public Vector3 Dir;//Current Rotation
    public Vector3 posZ;//Current Position
    public float Time;//CurrentTime
    public CurlingthrowData(Vector3 Dir1, Vector3 posZ1, float Time1)
    {
        Dir = Dir1;
        posZ = posZ1;
        Time = Time1;
    }
}
[Serializable]
public class L4RsultData
{
    public CurlingthrowData[] ThrowDatas1;
    public Vector3 CurlingStopPosition;
    public Vector3 CurlingStopEulerAngles;
    public float StopTime;
    public float Score;
    public L4RsultData(CurlingthrowData[] ThrowDatas11, Vector3 CurlingStopPosition1, Vector3 CurlingStopEulerAngles1, float StopTime1, float Score1)
    {
        CurlingStopPosition = CurlingStopPosition1;
        ThrowDatas1 = ThrowDatas11;
        CurlingStopEulerAngles = CurlingStopEulerAngles1;
        StopTime = StopTime1;
        Score = Score1;

    }
}
[Serializable]
public class Strenth
{
    public L4RsultData[] L4RsultData;
}

public class Test_Graph : MonoBehaviour
{
    public Strenth strenth;


    public float health;
    void Start()
    {
        strenth = new Strenth();


        Vector3 temp = new Vector3(0, 2, 2);
        Vector3 temp1 = new Vector3(2, 2, 2);
        float sa = 2;

        CurlingthrowData temp2 = new CurlingthrowData(temp, temp1, sa);
        CurlingthrowData[] temp3 = new CurlingthrowData[1];
        temp3[0] = temp2;
        Vector3 curliingstopPosition = new Vector3(0, 0, 0);
        Vector3 CurlingStopEulerAngles = new Vector3(0, 0, 0);
        float stoptime = 2;
        float score = 30;

        strenth.L4RsultData = new L4RsultData[1];
        strenth.L4RsultData[0] = new L4RsultData(temp3, curliingstopPosition, CurlingStopEulerAngles, stoptime, score);


        string a = JsonUtility.ToJson(strenth);
        print(a);//最后运行一下

    }



    public void Update()
    {

    }

}