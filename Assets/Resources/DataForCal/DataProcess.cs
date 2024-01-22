using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataProcess : MonoBehaviour
{
    [Serializable]
    public class RecordData
    {
        public OneTeleport[] teleports;
    }

    [Serializable]
    public class OneTeleport
    {
        public string time;//跳转时刻
        public string teleportTime;//决策时间
        public string teleportType;
        public string exhibition;
        public Vector3[] prePositions;
        public Vector3[] nextPositons;
        public Vector3[] preForwards;
        public Vector3[] nextForwards;
        public double[] offsetAngles;
        //只有跳转到展品才有
        public Quality[] qualities;
        public double[] finalQualities;

        public OneTeleport(string time, Vector3[] prePositions, Vector3[] nextPositons, Vector3[] preForwards, Vector3[] nextForwards, double[] offsetAngles, string teleportTime, string teleportType)
        {
            this.time = time;
            this.prePositions = prePositions;
            this.preForwards = preForwards;
            this.nextPositons = nextPositons;
            this.nextForwards = nextForwards;
            this.offsetAngles = offsetAngles;
            this.teleportTime = teleportTime;
            this.teleportType = teleportType;
        }
    }

    public RecordData recordData1;
    public RecordData recordData2;
    public RecordData recordData3;

    public void Awake()
    {
        string path1 = Application.dataPath + "/Resources/DataForCal/SHXWZT_5.json";
        string path2 = Application.dataPath + "/Resources/DataForCal/SHXWZT_10.json";
        string path3 = Application.dataPath + "/Resources/DataForCal/SHXWZT_15.json";
        string jsonFromFile1 = File.ReadAllText(path1);
        recordData1 = JsonUtility.FromJson<RecordData>(jsonFromFile1);
        string jsonFromFile2 = File.ReadAllText(path2);
        recordData2 = JsonUtility.FromJson<RecordData>(jsonFromFile2);
        string jsonFromFile3 = File.ReadAllText(path3);
        recordData3 = JsonUtility.FromJson<RecordData>(jsonFromFile3);

        ProcessData(recordData1);
        Debug.Log("---------------------");
        ProcessData(recordData2);
        Debug.Log("---------------------");
        ProcessData(recordData3);

    }

    void ProcessData(RecordData recordData)
    {
        int index = 0;
        for(int i = 0; i< 4; i++)
        {
            double timeSum = 0;
            double scoreSum = 0;
            double angleSum = 0;
            int num = 0;
            for (int j = 0; j < 5; j++)
            {
                OneTeleport oneTeleport = recordData.teleports[index];
                timeSum += double.Parse(oneTeleport.teleportTime);
                num = oneTeleport.finalQualities.Length;
                foreach(var v in oneTeleport.finalQualities)
                {
                    scoreSum += v;
                }
                foreach(var v in oneTeleport.offsetAngles)
                {
                    angleSum += v;
                }


                index++;
            }
            Debug.Log("第" + index + "个:time:" + timeSum / 5 + ",score:" + scoreSum / (5 * num) + ",angle:" + angleSum / (5 * num));
        }
    }

}
