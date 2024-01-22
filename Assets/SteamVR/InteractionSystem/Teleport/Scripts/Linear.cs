using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Linear : BasicFormation
{
    public float distance = 1;
    public float minDistance = 0.5f;
    public float maxDistance = 2f;

    public override void MakeFormation() {
        balls = new GameObject[member_num];
        lines = new GameObject[member_num-1];

        float left_x = -(member_num - 1) * distance / 2;
        for (int i = 0; i < member_num; i++) {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.localScale = Vector3.one * ballRadius;
            //ball.GetComponent<Renderer>().material.color = ballColor;
            ball.transform.parent = transform;
            ball.transform.localPosition = new Vector3(left_x + distance * i, 0.01f, 0);
            balls[i] = ball;
        }
        for(int i=0;i<member_num - 1; i++)
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

    public override void UpdateFormation()
    {
        if (true)
        {//todo
            bool rotateLeft = rotateLeftAction.GetState(SteamVR_Input_Sources.LeftHand);
            bool rotateRight = rotateRightAction.GetState(SteamVR_Input_Sources.LeftHand);
            bool largen = largenAction.GetState(SteamVR_Input_Sources.LeftHand);
            bool lessen = lessenAction.GetState(SteamVR_Input_Sources.LeftHand);
            if (largen)
            {
                Largen();
            }
            else if (lessen)
            {
                Lessen();
            }
            if (rotateLeft)
            {
                RotateLeft();
            }
            else if (rotateRight)
            {
                RotateRight();
            }
        }

        Vector3 forward;
        if (member_num > 1)
        {
            Vector3 line = balls[1].transform.position - balls[0].transform.position;
            forward = new Vector3(-line.z, 0, line.x);
        }
        else
        {
            forward = Vector3.forward;
        }

        for (int i = 0; i < member_num; i++)
        {
            balls[i].transform.forward = forward;
            if (i != member_num - 1) {
                LineRenderer lineRenderer = lines[i].GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, balls[i].transform.position);
                lineRenderer.SetPosition(1, balls[i+1].transform.position);
            }
            
        }
    }
    public override void Largen() {
        if(distance > maxDistance)
        {
            return;
        }
        int sensitivity = transform.parent.gameObject.GetComponent<DestinationFormation>().scaleSensitivity;
        distance += (maxDistance - minDistance) * 0.0001f * sensitivity;
        UpdateScale();
    }

    public override void Lessen() {
        if (distance < minDistance)
        {
            return;
        }
        int sensitivity = transform.parent.gameObject.GetComponent<DestinationFormation>().scaleSensitivity;
        distance -= (maxDistance - minDistance) * 0.0001f * sensitivity;
        UpdateScale();
    }

    void UpdateScale()
    {
        float left_x = -(member_num - 1) * distance / 2;
        for (int i = 0; i < member_num; i++) { 
            balls[i].transform.localPosition = new Vector3(left_x + distance * i, 0.01f, 0);
        }
    }

    public override void CheckValidation()
    {
        for (int i = 0; i < member_num-1; i++)
        {
            lines[i].GetComponent<LineRenderer>().material.color = Color.black;
        }
        for (int i = 0; i < member_num; i++)
        {
            bool isValid = false;
            Collider[] colliders = Physics.OverlapSphere(new Vector3(balls[i].transform.position.x, balls[i].transform.position.y - 0.1f, balls[i].transform.position.z), 0.05f);
            if (colliders.Length != 0)
            {
                isValid = true;
            }
            if (!isValid)
            {
                if (i < member_num - 1) {
                    lines[i].GetComponent<LineRenderer>().material.color = Color.red;
                }
                if (i > 0)
                {
                    lines[(i - 1 + member_num) % member_num].GetComponent<LineRenderer>().material.color = Color.red;
                }
                
            }
        }
    }

}
