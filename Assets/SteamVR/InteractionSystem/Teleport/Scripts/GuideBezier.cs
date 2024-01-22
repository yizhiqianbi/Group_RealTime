using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum RenderingMode
{
    Opaque,
    Cutout,
    Fade,
    Transparent,
}
public class GuideBezier : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnActiveChanged))]
    public bool active;
    [SyncVar(hook = nameof(OnPositionChanged))]
    public Vector3 position1;
    [SyncVar(hook = nameof(OnPositionChanged))]
    public Vector3 position2;
    [SyncVar(hook = nameof(OnPositionChanged))]
    public Vector3 position3;

    private LineRenderer lineRenderer;
    private int layerOrder = 0;
    private int _segmentNum = 50;

    public void OnActiveChanged(bool oldActive, bool newActive)
    {
        if (newActive)
        {
            lineRenderer.enabled = true;
            OnPositionChanged(position3, position3);
            if (isServer)
            {
                //透明化
                /*RaycastHit hitInfo;
                Physics.Linecast(position1, position3, out hitInfo);
                //Renderer renderer = hitInfo.transform.GetComponent<Renderer>();
                //renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.5f);
                SetMaterialRenderingMode(hitInfo.transform.GetComponent<MeshRenderer>().material, RenderingMode.Transparent);
                transform.GetComponent<MeshRenderer>().material.color = new Color(
                    transform.GetComponent<MeshRenderer>().material.color.r,
                    transform.GetComponent<MeshRenderer>().material.color.g,
                    transform.GetComponent<MeshRenderer>().material.color.b,
                    0.5f);*/
            }
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    Vector3[] points;
    /*    public void OnPosition3Changed(Vector3 oldP3, Vector3 newP3)
        {
            //因为每次都是依次改变p1,p2,p3,所以只需要p3变了再去更新即可
            if (active)
            {
                points = BezierUtils.GetCubicBeizerList(position1, position2, newP3, _segmentNum);
                lineRenderer.positionCount = (_segmentNum);
                lineRenderer.SetPositions(points);
            }
        }*/

    public void OnPositionChanged(Vector3 oldP, Vector3 newP)
    {
        //因为每次都是依次改变p1,p2,p3,所以只需要p3变了再去更新即可
        if (active)
        {
            points = BezierUtils.GetCubicBeizerList(position1, position2, position3, _segmentNum);
            lineRenderer.positionCount = (_segmentNum);
            lineRenderer.SetPositions(points);
        }
    }

    void Start()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerID = layerOrder;

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
                break;
        }
    }
}
