using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ColorScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnColorChanged))]
    public Color color;

    [Command]
    public void CmdChangeColor()
    {
        var tempColor = new Color
        (
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            1
        );
        color = tempColor;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public void OnColorChanged(Color oldColor, Color newColor)
    {
        FindChildRecursively(transform, "Shirt").gameObject.GetComponent<MeshRenderer>().material.color = newColor;
        FindChildRecursively(transform, "StandPoint").gameObject.GetComponent<MeshRenderer>().material.color = newColor;
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer != null)
        {
            lineRenderer.material.color = newColor;
        }
    }

    public Color GetColor()
    {
        //return this.gameObject.transform.Find("Head/Shirt").gameObject.GetComponent<MeshRenderer>().material.color;
        return FindChildRecursively(transform,"Shirt").gameObject.GetComponent<MeshRenderer>().material.color;
    }

    // 在当前父物体下递归查找子物体
    Transform FindChildRecursively(Transform parent, string childName)
    {
        // 遍历当前父物体的所有子物体
        foreach (Transform child in parent)
        {
            // 检查子物体的名称是否与所需名称相同
            if (child.name == childName)
            {
                // 找到了子物体，返回该子物体的Transform组件
                return child;
            }
            else
            {
                // 没有找到子物体，递归查找当前子物体的子物体
                Transform foundChild = FindChildRecursively(child, childName);
                if (foundChild != null)
                {
                    // 找到了子物体，返回该子物体的Transform组件
                    return foundChild;
                }
            }
        }

        // 没有找到子物体，返回空值
        return null;
    }
}
