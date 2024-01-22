using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BasicFormation : MonoBehaviour
{
    public float ballRadius = 0.05f;
    //public Color ballColor = Color.blue;
    public Material lineMaterial;
    public GameObject guide;
    public List<GameObject> followers;
    public int member_num;
    public GameObject[] balls;
    public GameObject[] lines;
    public Dictionary<GameObject, GameObject> map;
    public SteamVR_Action_Boolean rotateLeftAction;
    public SteamVR_Action_Boolean rotateRightAction;
    public SteamVR_Action_Boolean largenAction;
    public SteamVR_Action_Boolean lessenAction;

    public void Awake()
    {
        rotateLeftAction = transform.parent.gameObject.GetComponent<DestinationFormation>().rotateLeftAction;
        rotateRightAction = transform.parent.gameObject.GetComponent<DestinationFormation>().rotateRightAction;
        largenAction = transform.parent.gameObject.GetComponent<DestinationFormation>().largenAction;
        lessenAction = transform.parent.gameObject.GetComponent<DestinationFormation>().lessenAction;

        guide = GameObject.Find("Guide");
        GameObject Followers = GameObject.Find("Followers");
        member_num = Followers.transform.childCount + 1;
        for(int i = 0; i < member_num - 1; i++)
        {
            followers.Add(Followers.transform.GetChild(i).gameObject);
        }
        map = new Dictionary<GameObject, GameObject>();
        MakeFormation();
        MapAvatars();
    }

    public void Update()
    {
        UpdateFormation();
        CheckValidation();
    }

    public virtual void MakeFormation() { }

    public void MapAvatars()
    {
        Vector3 localPosition = new Vector3(0, 1.7f, 0);
        GameObject avatar;
        //map
        GameObject guide_avatar = GameObject.Instantiate(guide);
        map.Add(balls[0], guide_avatar);
        balls[0].GetComponent<MeshRenderer>().material.color = guide_avatar.GetComponent<GetColor>().getColor();
        for (int i = 0; i < member_num - 1; i++)
        {
            GameObject follower_avatar = GameObject.Instantiate(followers[i]);
            map.Add(balls[i + 1], follower_avatar);
            balls[i + 1].GetComponent<MeshRenderer>().material.color = follower_avatar.GetComponent<GetColor>().getColor();
        }

        //set position
        foreach(GameObject ball in map.Keys)
        {
            map.TryGetValue(ball, out avatar);
            avatar.transform.parent = ball.transform;
            avatar.transform.position = new Vector3(ball.transform.position.x, 1.7f, ball.transform.position.z);
        }
    }
    
    public virtual void UpdateFormation() {
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
        for (int i = 0; i < member_num; i++)
        {
            balls[i].transform.forward = this.transform.position - balls[i].transform.position;
            LineRenderer lineRenderer = lines[i].GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, balls[i].transform.position);
            lineRenderer.SetPosition(1, balls[(i + 1) % member_num].transform.position);
        }
    }

    public virtual void Largen() { }

    public virtual void Lessen() { }
    
    public void RotateRight()
    {
        int sensitivity = transform.parent.gameObject.GetComponent<DestinationFormation>().rotateSensitivity;
        transform.eulerAngles += new Vector3(0, 360f / member_num / 2000 * sensitivity, 0);
    }

    public void RotateLeft()
    {
        int sensitivity = transform.parent.gameObject.GetComponent<DestinationFormation>().rotateSensitivity;
        transform.eulerAngles -= new Vector3(0, 360f / member_num / 2000 * sensitivity, 0);
    }


    public virtual void CheckValidation() {
        for(int i = 0; i < member_num; i++)
        {
            lines[i].GetComponent<LineRenderer>().material.color = Color.black;
        }
        for(int i = 0; i < member_num; i++)
        {
            bool isValid = false;
            Collider[] colliders = Physics.OverlapSphere(new Vector3(balls[i].transform.position.x, balls[i].transform.position.y - 0.1f, balls[i].transform.position.z), 0.05f);
            if (colliders.Length != 0) {
                isValid = true;
            }
            if (!isValid)
            {
                lines[i].GetComponent<LineRenderer>().material.color = Color.red;
                lines[(i - 1 + member_num) % member_num].GetComponent<LineRenderer>().material.color = Color.red;
            }
        }
    }

    public List<Transform> GetAllTransforms() {
        List<Transform> transforms = new List<Transform>();
        for(int i = 0; i < member_num; i++)
        {
            transforms.Add(balls[i].transform);
        }
        return transforms;
    }

    public Transform GetGuideTransform()
    {
        return balls[0].transform;
    }
}
