using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class CrownScript : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnRemoveCrownChanged))]
    public bool removeCrown = false;

    public void OnRemoveCrownChanged(bool oldVar, bool newVar)
    {
        if (newVar)
        {
            Transform crown = FindChildRecursively(transform, "Crown");
            if (crown)
            {
                Destroy(crown.gameObject);
            }
        }
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
