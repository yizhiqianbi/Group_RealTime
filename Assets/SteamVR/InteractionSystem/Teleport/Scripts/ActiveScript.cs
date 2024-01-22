using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ActiveScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnActiveChanged))]
    public bool active;

    public void Awake()
    {
        active = true;
    }
    public void PreAvatarSetActive(bool active)
    {
        this.active = active;
    }

    public void OnActiveChanged(bool oldActive, bool newActive)
    {
        transform.Find("Head").gameObject.SetActive(newActive);
        FindChildRecursively(transform, "StandPoint").gameObject.SetActive(newActive);
    }

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
