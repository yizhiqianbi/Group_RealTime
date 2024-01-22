﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
public class ExportNavMesh
{
    [MenuItem("NavMesh/Export")]
    static void Export()
    {
        Debug.Log("ExportNavMesh");

        NavMeshTriangulation tmpNavMeshTriangulation = NavMesh.CalculateTriangulation();

        //新建文件
        string tmpPath = Application.dataPath + "/NavMeshObj/" + SceneManager.GetActiveScene().name + ".obj";
        Debug.Log(tmpPath);
        StreamWriter tmpStreamWriter = new StreamWriter(tmpPath);

        //顶点
        for (int i = 0; i < tmpNavMeshTriangulation.vertices.Length; i++)
        {
            tmpStreamWriter.WriteLine("v  " + tmpNavMeshTriangulation.vertices[i].x + " " + tmpNavMeshTriangulation.vertices[i].y + " " + tmpNavMeshTriangulation.vertices[i].z);

        }

        tmpStreamWriter.WriteLine("g pPlane1");

        //索引
        for (int i = 0; i < tmpNavMeshTriangulation.indices.Length;)
        {
            tmpStreamWriter.WriteLine("f " + (tmpNavMeshTriangulation.indices[i] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 1] + 1) + " " + (tmpNavMeshTriangulation.indices[i + 2] + 1));
            i = i + 3;
        }

        tmpStreamWriter.Flush();
        tmpStreamWriter.Close();

        Debug.Log("ExportNavMesh Success");
    }
}