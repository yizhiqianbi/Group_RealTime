using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCircle : MonoBehaviour
{
    public bool drawCircle;
    public float range;
    public Material material;
    private LineRenderer lineRenderer;

    public void Awake()
    {
        if (!drawCircle) return;
        if (!lineRenderer)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = material;
        }
        Vector3 center = transform.position;
        float radius = range;
        int cnt = 180;
        lineRenderer.positionCount = cnt + 2;
        lineRenderer.startWidth = .1f;
        lineRenderer.endWidth = .1f;
        for (int i = 0; i < cnt; i++)
        {
            float x = center.x + radius * Mathf.Cos(i * 360 / cnt * Mathf.PI / 180f);
            float z = center.z + radius * Mathf.Sin(i * 360 / cnt * Mathf.PI / 180f);
            lineRenderer.SetPosition(i, new Vector3(x, transform.position.y, z));
        }
        float xx = center.x + radius * Mathf.Cos(0);
        float zz = center.z + radius * Mathf.Sin(0);
        lineRenderer.SetPosition(cnt, new Vector3(xx, transform.position.y, zz));
        float xxx = center.x + radius * Mathf.Cos(1 * 360 / cnt * Mathf.PI / 180f);
        float zzz = center.z + radius * Mathf.Sin(1 * 360 / cnt * Mathf.PI / 180f);
        lineRenderer.SetPosition(cnt + 1, new Vector3(xxx, transform.position.y, zzz));
        /*        ReadRecommendedFormation();//加载推荐队形
                Debug.Log("加载完毕");*/
    }
}
