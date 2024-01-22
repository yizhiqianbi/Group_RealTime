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

    // �ڵ�ǰ�������µݹ����������
    Transform FindChildRecursively(Transform parent, string childName)
    {
        // ������ǰ�����������������
        foreach (Transform child in parent)
        {
            // ���������������Ƿ�������������ͬ
            if (child.name == childName)
            {
                // �ҵ��������壬���ظ��������Transform���
                return child;
            }
            else
            {
                // û���ҵ������壬�ݹ���ҵ�ǰ�������������
                Transform foundChild = FindChildRecursively(child, childName);
                if (foundChild != null)
                {
                    // �ҵ��������壬���ظ��������Transform���
                    return foundChild;
                }
            }
        }

        // û���ҵ������壬���ؿ�ֵ
        return null;
    }
}
