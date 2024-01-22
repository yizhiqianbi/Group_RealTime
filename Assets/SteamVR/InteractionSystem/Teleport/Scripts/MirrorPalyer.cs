using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MirrorPalyer : NetworkBehaviour
{
    public GameObject player;
    public GameObject teleporting;
    private LineRenderer lineRenderer;
    private int layerOrder = 0;
    private int _segmentNum = 50;

    /*    [TargetRpc]
        public void TargetTeleport(NetworkConnection targetConnection,Transform targetTransform,int num)
        {
            *//*        transform.position = targetTransform.position;
                    transform.eulerAngles = targetTransform.eulerAngles;*/
    /*        Transform VRCameraTrans = transform.Find("Player/SteamVRObjects/VRCamera");
            Transform LeftHandTrans = transform.Find("Player/SteamVRObjects/LeftHand");
            Transform RightHandTrans = transform.Find("Player/SteamVRObjects/RightHand");
            Vector3 leftHandOffset = LeftHandTrans.position - VRCameraTrans.position;
            Vector3 rightHandOffset = RightHandTrans.position - VRCameraTrans.position;

            VRCameraTrans.position = targetTransform.position;
            VRCameraTrans.eulerAngles = targetTransform.eulerAngles;
            LeftHandTrans.position = VRCameraTrans.position + leftHandOffset;
            RightHandTrans.position = VRCameraTrans.position + rightHandOffset;*//*

            Transform VRCameraTrans = transform.Find("Player/SteamVRObjects/VRCamera");
            Transform par = VRCameraTrans.parent;
            var lerp = VRCameraTrans.position - par.position;
            par.position = targetTransform.position - lerp;
            //VRCameraTrans.position = par.position + lerp;


            lerp = VRCameraTrans.eulerAngles - par.eulerAngles;

            par.eulerAngles = new Vector3(par.eulerAngles.x, targetTransform.eulerAngles.y - lerp.y, par.eulerAngles.z);
            //par.eulerAngles.y = targetTransform.eulerAngles.y - lerp.y;
    *//*
            Quaternion deltaQ = par.rotation * Quaternion.Inverse(VRCameraTrans.rotation);
            float temp = Mathf.Min(Mathf.Max(deltaQ.w, -1.0f), 1.0f);//将w控制在-1.0-1.0之间。
            float angleQua = 2 * Mathf.Acos(temp);
            angleQua = angleQua * Mathf.Rad2Deg;
            Quaternion VRcameraAfter=*//*


        }*/

    [TargetRpc]
    public void TargetTeleport(NetworkConnection targetConnection, Vector3 position, Vector3 eulerAngles, int num)
    {
        /*        transform.position = targetTransform.position;
                transform.eulerAngles = targetTransform.eulerAngles;*/
        /*        Transform VRCameraTrans = transform.Find("Player/SteamVRObjects/VRCamera");
                Transform LeftHandTrans = transform.Find("Player/SteamVRObjects/LeftHand");
                Transform RightHandTrans = transform.Find("Player/SteamVRObjects/RightHand");
                Vector3 leftHandOffset = LeftHandTrans.position - VRCameraTrans.position;
                Vector3 rightHandOffset = RightHandTrans.position - VRCameraTrans.position;

                VRCameraTrans.position = targetTransform.position;
                VRCameraTrans.eulerAngles = targetTransform.eulerAngles;
                LeftHandTrans.position = VRCameraTrans.position + leftHandOffset;
                RightHandTrans.position = VRCameraTrans.position + rightHandOffset;*/

        Transform VRCameraTrans = transform.Find("Player/SteamVRObjects/VRCamera");
        Transform par = VRCameraTrans.parent;
        var lerp = VRCameraTrans.position - par.position;
        par.position = position - lerp;
        //VRCameraTrans.position = par.position + lerp;


        lerp = VRCameraTrans.eulerAngles - par.eulerAngles;

        par.eulerAngles = new Vector3(par.eulerAngles.x, eulerAngles.y - lerp.y, par.eulerAngles.z);
        //par.eulerAngles.y = targetTransform.eulerAngles.y - lerp.y;
        /*
                Quaternion deltaQ = par.rotation * Quaternion.Inverse(VRCameraTrans.rotation);
                float temp = Mathf.Min(Mathf.Max(deltaQ.w, -1.0f), 1.0f);//将w控制在-1.0-1.0之间。
                float angleQua = 2 * Mathf.Acos(temp);
                angleQua = angleQua * Mathf.Rad2Deg;
                Quaternion VRcameraAfter=*/


    }

    [TargetRpc]
    public void TargetUpdateBeizer(NetworkConnection targetConnection, Vector3 preAvatarPosition, bool active) {
        if (active)
        {
            lineRenderer.enabled = true;
            DrawCubicPowerCurve(preAvatarPosition);
            //透明化
/*            Vector3 start = transform.Find("Player/SteamVRObjects/RightHand").position;
            RaycastHit hitInfo;
            Physics.Linecast(start, preAvatarPosition, out hitInfo);
            Renderer renderer = hitInfo.transform.GetComponent<Renderer>();
            renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0.5f);*/
        }
        else
        {
            lineRenderer.enabled = false;
        }
        
    }

    Vector3[] points;
    void DrawCubicPowerCurve(Vector3 preAvatarPosition)
    {
        Vector3 p1 = transform.Find("Player/SteamVRObjects/RightHand").position;
        Vector3 p2 = p1 + transform.Find("Player/SteamVRObjects/RightHand").forward * 1;
        points = BezierUtils.GetCubicBeizerList(p1, p2, preAvatarPosition, _segmentNum);
        // 设置 LineRenderer 的点个数，并赋值点值
        lineRenderer.positionCount = (_segmentNum);
        lineRenderer.SetPositions(points);
    }

    void Start()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerID = layerOrder;

    }

    public void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            player.SetActive(false);
            teleporting.SetActive(false);
            return;
        }
        else
        {
            player.SetActive(true);
            teleporting.SetActive(true);
            //完成位置匹配
            Transform VRCameraTrans = transform.Find("Player/SteamVRObjects/VRCamera");
            transform.Find("Avatar").gameObject.transform.position = VRCameraTrans.position;
            transform.Find("Avatar").gameObject.transform.eulerAngles = VRCameraTrans.eulerAngles;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            GetComponent<ColorScript>().CmdChangeColor();
        }
        var moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110f;
        var moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4.0f;

        transform.Rotate(0, moveX, 0);
        transform.Translate(0, 0, moveZ);

    }

}
