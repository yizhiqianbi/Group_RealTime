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
