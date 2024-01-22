using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Circular : BasicFormation
{
    
    public float radius = 1.5f;
    public float minRadius = 1f;
    public float maxRadius = 2.5f;
    private float angle;

    public override void  MakeFormation() {
        balls = new GameObject[member_num];
        lines = new GameObject[member_num];
        angle = 360f / member_num;

        for (int i = 0; i < member_num; i++)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.localScale = Vector3.one * ballRadius;
            //ball.GetComponent<Renderer>().material.color = ballColor;
            ball.transform.parent = transform;

            float x = radius * Mathf.Cos(Mathf.Deg2Rad * i * angle);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * i * angle);
            Vector3 position = new Vector3(x, 0.01f, z);
            ball.transform.localPosition = position;
            balls[i] = ball;

        }
        for(int i = 0; i < member_num; i++)
        {
            GameObject line_i = new GameObject("line_" + i);
            GameObject ball_i = balls[i];
            GameObject ball_next = balls[(i + 1) % member_num];
            line_i.transform.parent = ball_i.transform;
            LineRenderer lineRenderer_i = line_i.AddComponent<LineRenderer>();
            lineRenderer_i.material = lineMaterial;
            lineRenderer_i.material.color = Color.black;
            lineRenderer_i.startWidth = ballRadius / 4;
            lineRenderer_i.endWidth = ballRadius / 4;
            lineRenderer_i.SetPosition(0, ball_i.transform.position);
            lineRenderer_i.SetPosition(1, ball_next.transform.position);
            lines[i] = line_i;
        }

    }

    public override void Largen() {
        if (radius >= maxRadius) {
            return;
        }
        int sensitivity = transform.parent.gameObject.GetComponent<DestinationFormation>().scaleSensitivity;
        radius += (maxRadius - minRadius) * 0.0001f*  sensitivity;
        UpdateScale();
    }

    public override void Lessen() {
        if (radius <= minRadius) {
            return;
        }
        int sensitivity = transform.parent.gameObject.GetComponent<DestinationFormation>().scaleSensitivity;
        radius -= (maxRadius - minRadius) * 0.0001f * sensitivity;
        UpdateScale();
    }
    private void UpdateScale()
    {
        for (int i = 0; i < member_num; i++)
        {
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * i * angle);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * i * angle);
            Vector3 position = new Vector3(x, 0.01f, z);
            balls[i].transform.localPosition = position;
        }
    }

}