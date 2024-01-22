using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Opaque : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnRenderingModeChanged))]
    public int renderingModeValue;

    public void OnRenderingModeChanged(int oldRenderingMode, int newRenderingMode)
    {
        if (newRenderingMode == oldRenderingMode) return;
        RenderingMode renderingMode = (RenderingMode)newRenderingMode;
        Material headMat = FindChildRecursively(transform, "Head").GetComponent<MeshRenderer>().material;
        Material HMDMat = FindChildRecursively(transform, "HMD").GetComponent<MeshRenderer>().material;
        Material shirtMat = FindChildRecursively(transform, "Shirt").GetComponent<MeshRenderer>().material;
        Material standPointMat = FindChildRecursively(transform, "StandPoint").GetComponent<MeshRenderer>().material;
        SetMaterialRenderingMode(headMat, renderingMode);
        SetMaterialRenderingMode(HMDMat, renderingMode);
        SetMaterialRenderingMode(shirtMat, renderingMode);
        SetMaterialRenderingMode(standPointMat, renderingMode);
        Transform preArrow = FindChildRecursively(transform, "PreArrow");
        if (preArrow)
        {
            for(int i = 0; i < preArrow.childCount; i++)
            {
                Material cubeMat = preArrow.GetChild(i).GetComponent<MeshRenderer>().material;
                SetMaterialRenderingMode(cubeMat, renderingMode);
            }
        }
        Transform nowArrow = FindChildRecursively(transform, "NowArrow");
        if (nowArrow)
        {
            for (int i = 0; i < nowArrow.childCount; i++)
            {
                Material cubeMat = nowArrow.GetChild(i).GetComponent<MeshRenderer>().material;
                SetMaterialRenderingMode(cubeMat, renderingMode);
            }
        }
    }

    public void SetMaterialRenderingMode(Material material, RenderingMode renderingMode)
    {
        switch (renderingMode)
        {
            case RenderingMode.Opaque:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case RenderingMode.Cutout:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case RenderingMode.Fade:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case RenderingMode.Transparent:
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                material.color = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
                break;
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
