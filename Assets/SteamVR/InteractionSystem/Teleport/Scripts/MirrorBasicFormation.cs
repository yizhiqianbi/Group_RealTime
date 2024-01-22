using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Valve.VR;
using System;
using System.IO;

public class MirrorBasicFormation : NetworkBehaviour
{
    [Serializable]
    public class RecordData
    {
        public OneTeleport[] teleports;
    }

    [Serializable]
    public class OneTeleport
    {
        public string time;//跳转时刻
        public string teleportTime;//决策时间
        public string teleportType;
        public string exhibition;
        public Vector3[] prePositions;
        public Vector3[] nextPositons;
        public Vector3[] preForwards;
        public Vector3[] nextForwards;
        public double[] offsetAngles;
        //只有跳转到展品才有
        public Quality[] qualities;
        public double[] finalQualities;

        public OneTeleport(string time, Vector3[] prePositions, Vector3[] nextPositons, Vector3[] preForwards, Vector3[] nextForwards, double[] offsetAngles, string teleportTime, string teleportType)
        {
            this.time = time;
            this.prePositions = prePositions;
            this.preForwards = preForwards;
            this.nextPositons = nextPositons;
            this.nextForwards = nextForwards;
            this.offsetAngles = offsetAngles;
            this.teleportTime = teleportTime;
            this.teleportType = teleportType;
        }
    }

    public SteamVR_Action_Boolean rotateLeftAction;
    public SteamVR_Action_Boolean rotateRightAction;
    public SteamVR_Action_Boolean largenAction;
    public SteamVR_Action_Boolean lessenAction;

    public GameObject PreAvatarPrefeb;
    public GameObject LinePrefeb;
    public List<GameObject> preAvatars;
    public GameObject[] lines;
    public float offset = 1.7f;//落点到人物的偏移
    public Dictionary<int, NetworkConnection> index2connectionMap;
    public Dictionary<int, GameObject> index2simulatorMap;
    public MirrorDestinationFormation mirrorDestinationFormation;
    public GameObject cameraContainer;
    private string path;
    public string formationType;
    //private float timer;
    public void Initialize(int mem_num)
    {
        if (!isServer)
        {
            return;
        }
        rotateLeftAction = mirrorDestinationFormation.rotateLeftAction;
        rotateRightAction = mirrorDestinationFormation.rotateRightAction;
        largenAction = mirrorDestinationFormation.largenAction;
        lessenAction = mirrorDestinationFormation.lessenAction;

        PreAvatarPrefeb = mirrorDestinationFormation.PreAvatarPrefeb;
        LinePrefeb = mirrorDestinationFormation.LinePrefeb;
        index2connectionMap = new Dictionary<int, NetworkConnection>();
        index2simulatorMap = mirrorDestinationFormation.index2simulatorMap;
        bool isRecommendedFormation = transform.gameObject.CompareTag("RecommendedFormation");
        foreach (NetworkConnection connection in netIdentity.observers.Values)
        {
            GameObject preAvatar = Instantiate(PreAvatarPrefeb);
            NetworkServer.Spawn(preAvatar);
            //颜色需要根据connection中的颜色变化
            preAvatar.GetComponent<ColorScript>().SetColor(connection.identity.gameObject.GetComponent<ColorScript>().GetColor());
            preAvatar.transform.Find("Head/NowArrow").GetComponent<ArrowScript>().SetArrowColor(connection.identity.gameObject.GetComponent<ColorScript>().GetColor());
            preAvatar.transform.Find("Head/NowArrow").gameObject.SetActive(mirrorDestinationFormation.showArrow);
            preAvatar.transform.Find("Head/PreArrow").gameObject.SetActive(mirrorDestinationFormation.showArrow);
            ChangeLayer(preAvatar.transform, "JustForShowing");
            //preAvatar.GetComponent<PreferenceScript>().SetPreference(connection.identity.gameObject.GetComponent<PreferenceScript>().GetPreference());
            if (!isRecommendedFormation)//如果是RecommendedFormation，preAvatar不需要随父物体移动
            {
                RpcSetParent(connection.connectionId);
            }
            else
            {
                RpcRename(connection.connectionId);
            }

            //preAvatar.GetComponent<ActiveScript>().PreAvatarSetActive(false);
            preAvatars.Add(preAvatar);
            index2connectionMap[preAvatars.IndexOf(preAvatar)] = connection;
        }
        //模拟用户的preAvatar
        for (int i = netIdentity.observers.Count; i < mem_num; i++)
        {
            //模拟用户的预览形象
            GameObject preAvatar = Instantiate(PreAvatarPrefeb);
            NetworkServer.Spawn(preAvatar);
            //preAvatar.GetComponent<ColorScript>().SetColor(Color.blue);
            preAvatar.GetComponent<ColorScript>().SetColor(index2simulatorMap[i].GetComponent<ColorScript>().color);

            preAvatar.transform.Find("Head/NowArrow").GetComponent<ArrowScript>().SetArrowColor(index2simulatorMap[i].GetComponent<ColorScript>().color);
            preAvatar.transform.Find("Head/NowArrow").gameObject.SetActive(mirrorDestinationFormation.showArrow);
            preAvatar.transform.Find("Head/PreArrow").gameObject.SetActive(mirrorDestinationFormation.showArrow);
            ChangeLayer(preAvatar.transform, "JustForShowing");
            if (!isRecommendedFormation)
            {
                RpcSetParent(i);
            }
            else
            {
                RpcRename(i);
            }
            //preAvatar.GetComponent<ActiveScript>().PreAvatarSetActive(false);
            preAvatars.Add(preAvatar);
        }
        //将除了导游的皇冠删除
        for (int i = 1; i < preAvatars.Count; i++)
        {
            RemoveCrown(preAvatars[i].transform);
        }
        MakeFormation();
        SetPreAvatarsActive(false);
    }

    void RemoveCrown(Transform player)
    {
        player.gameObject.GetComponent<CrownScript>().removeCrown = true;
    }

    [ClientRpc]
    private void RpcSetParent(int i)
    {
        GameObject preAvatar = GameObject.Find("PreAvatarPrefeb(Clone)");
        preAvatar.transform.SetParent(transform);
        preAvatar.transform.name = "preAvatar_" + i;
    }

    [ClientRpc]
    private void RpcRename(int i)
    {
        GameObject preAvatar = GameObject.Find("PreAvatarPrefeb(Clone)");
        preAvatar.transform.name = "preAvatar_" + i;
    }

    public virtual void MakeFormation() {
        foreach (GameObject preAvatar in preAvatars)
        {
            preAvatar.transform.localPosition = new Vector3(UnityEngine.Random.Range(1, 10), 0, UnityEngine.Random.Range(1, 10));
        }
    }

    void UpdateArrow()
    {
        for (int i = 0; i < preAvatars.Count; i++)
        {
            GameObject preAvatar = preAvatars[i];
            GameObject visitor = null;
            if (i < index2connectionMap.Count)//真实用户
            {
                visitor = index2connectionMap[i].identity.gameObject.transform.Find("Avatar").gameObject;
            }
            else
            {
                visitor = index2simulatorMap[i];
            }
            Vector3 preForward = visitor.transform.forward;
            preAvatar.transform.Find("Head/PreArrow").GetComponent<ArrowScript>().SetArrowForward(preForward);
        }
    }

    void UpdateOpaque()
    {
        RenderingMode renderingMode;
        if (mirrorDestinationFormation.isTeleporting) { renderingMode = RenderingMode.Transparent; }
        else { renderingMode = RenderingMode.Opaque; }
        for(int i = 0; i < index2connectionMap.Count; i++)//真实用户，需要客户端发起更新
        {
            NetworkConnection targetConnection = index2connectionMap[i];
            targetConnection.identity.gameObject.GetComponent<Opaque>().renderingModeValue = (int)renderingMode;
        }
        for(int i = index2connectionMap.Count; i < preAvatars.Count; i++)//模拟用户，服务器端更新即可
        {
            GameObject simulator = index2simulatorMap[i];
            simulator.GetComponent<Opaque>().renderingModeValue = (int)renderingMode;
        }
    }

    public void SetPreAvatarsActive(bool active)
    {
        foreach(GameObject preAvatar in preAvatars)
        {
            preAvatar.GetComponent<ActiveScript>().PreAvatarSetActive(active);
        }
        foreach(GameObject line in lines)
        {
            line.GetComponent<LineScript>().SetLineActive(active);
        }
        //更新射线
        int i = 0;
        foreach (NetworkConnection connection in netIdentity.observers.Values)
        {
            if (i == 0)
            {
                //Guide
                NetworkConnection targetConnection = index2connectionMap[i];
                //Vector3 position1 = targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().anchor1.position;
                Vector3 position1 = targetConnection.identity.gameObject.transform.Find("Player/SteamVRObjects/RightHand").position;
                //Vector3 position2 = targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().anchor2.position;
                Vector3 position2 = position1 + targetConnection.identity.gameObject.transform.Find("Player/SteamVRObjects/RightHand").forward * 3;
                Vector3 position3 = preAvatars[0].transform.Find("StandPoint").position;
                transform.parent.gameObject.GetComponent<GuideBezier>().active = active;
                transform.parent.gameObject.GetComponent<GuideBezier>().position1 = position1;
                transform.parent.gameObject.GetComponent<GuideBezier>().position2 = position2;
                transform.parent.gameObject.GetComponent<GuideBezier>().position3 = position3;
            }
            else
            {
                NetworkConnection targetConnection = index2connectionMap[i];
                targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().TargetUpdateBeizer(targetConnection, preAvatars[i].transform.Find("StandPoint").position, active);
            }
            i++;
        }

    }

/*    public void UpdateGuideBezier()
    {
        //更新射线
        int i = 0;
        foreach (NetworkConnection connection in netIdentity.observers.Values)
        {
            if (i == 0)
            {
                //Guide
                NetworkConnection targetConnection = index2connectionMap[i];
                //Vector3 position1 = targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().anchor1.position;
                Vector3 position1 = targetConnection.identity.gameObject.transform.Find("Player/SteamVRObjects/RightHand").position;
                //Vector3 position2 = targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().anchor2.position;
                Vector3 position2 = position1 + targetConnection.identity.gameObject.transform.Find("Player/SteamVRObjects/RightHand").forward * 1;
                Vector3 position3 = preAvatars[0].transform.Find("StandPoint").position;
                transform.parent.gameObject.GetComponent<GuideBezier>().active = mirrorDestinationFormation.isTeleporting;
                transform.parent.gameObject.GetComponent<GuideBezier>().position1 = position1;
                transform.parent.gameObject.GetComponent<GuideBezier>().position2 = position2;
                transform.parent.gameObject.GetComponent<GuideBezier>().position3 = position3;
            }
            else
            {
                NetworkConnection targetConnection = index2connectionMap[i];
                targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().TargetUpdateBeizer(targetConnection, preAvatars[i].transform.Find("StandPoint").position, mirrorDestinationFormation.isTeleporting);
            }
            i++;
        }
    }*/

    public virtual void UpdateFormation()
    {
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

        //更新朝向和线条
        int member_num = preAvatars.Count;
        for(int i = 0; i < member_num; i++)
        {
            Vector3 temp = transform.position - preAvatars[i].transform.position;
            preAvatars[i].transform.forward = new Vector3(temp.x, 0, temp.z);
            lines[i].GetComponent<LineScript>().SetStartPosition(preAvatars[i].transform.position - new Vector3(0, offset, 0));
            lines[i].GetComponent<LineScript>().SetEndPosition(preAvatars[(i + 1) % member_num].transform.position - new Vector3(0, offset, 0));
        }

        if(mirrorDestinationFormation.showArrow) UpdateArrow();//可视化，偏转角度
        UpdateOpaque();//更新透明度，在跳转透明，不在则不透明

        //更新模拟用户的朝向：看着预览导游
        if (mirrorDestinationFormation.isTeleporting && mirrorDestinationFormation.lookAtGuide)
        {
            for (int i = index2connectionMap.Count; i < preAvatars.Count; i++)
            {
                GameObject simulator = index2simulatorMap[i];
                Vector3 forward = preAvatars[0].transform.position - simulator.transform.position;
                simulator.transform.forward = new Vector3(forward.x, 0, forward.z);
            }
        }
    }
    
    public virtual void Largen() { }

    public virtual void Lessen() { }

    public void RotateRight()
    {
        int member_num = preAvatars.Count;
        transform.eulerAngles += new Vector3(0, 360f / 5 / 2000 * 100, 0);
    }

    public void RotateLeft()
    {
/*        int member_num = preAvatars.Count;
        List<Vector3> newPositions = new List<Vector3>();
        Vector3 oldEulerAngles = transform.eulerAngles;
        transform.eulerAngles -= new Vector3(0, 360f / member_num / 2000 * 50, 0);
        for(int i = 0; i< member_num; i++)
        {
            newPositions.Add(preAvatars[i].transform.position);
        }
        transform.eulerAngles = oldEulerAngles;
        for (int i = 0; i < member_num; i++)
        {
            preAvatars[i].transform.position = newPositions[i];
        }*/

        int member_num = preAvatars.Count;
        transform.eulerAngles -= new Vector3(0, 360f / 5 / 2000 * 100, 0);
    }

    public void TakeFollowers()
    {
        //记录分数，首先记录坐标，然后只需要计算分数
        string time = Time.time.ToString();
        Vector3[] prePositions = new Vector3[preAvatars.Count];
        Vector3[] nextPositons = new Vector3[preAvatars.Count];
        Vector3[] preForwards = new Vector3[preAvatars.Count];
        Vector3[] nextForwards = new Vector3[preAvatars.Count];
        double[] offsetAngles = new double[preAvatars.Count];
        for (int i = 0; i < preAvatars.Count; i++)
        {
            if (i < index2connectionMap.Count)//真实用户
            {
                prePositions[i] = index2connectionMap[i].identity.gameObject.transform.Find("Avatar").position;
                preForwards[i] = index2connectionMap[i].identity.gameObject.transform.Find("Avatar").forward;
            }
            else//模拟用户
            {
                prePositions[i] = index2simulatorMap[i].transform.position;
                preForwards[i] = index2simulatorMap[i].transform.forward;
            }
            nextPositons[i] = preAvatars[i].transform.position;
            nextForwards[i] = preAvatars[i].transform.forward;
            Vector3 a = new Vector3(preForwards[i].x, 0, preForwards[i].z);
            Vector3 b = new Vector3(nextForwards[i].x, 0, nextForwards[i].z);
            offsetAngles[i] = Mathf.Acos(Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b))) * Mathf.Rad2Deg;
        }
        string teleportTime = mirrorDestinationFormation.timer.ToString();
        mirrorDestinationFormation.timer = 0;
        string teleportType = formationType;

        OneTeleport oneTeleport = new OneTeleport(time, prePositions, nextPositons, preForwards, nextForwards, offsetAngles, teleportTime, teleportType);
        if(mirrorDestinationFormation.recordScore) RecordScore(oneTeleport);
        //
        int x=0;
        //跳转游客，排除自身
        for(int i = 1; i < preAvatars.Count; i++)
        {
            //真实用户
            if(i < index2connectionMap.Count)
            {
                NetworkConnection targetConnection = index2connectionMap[i];
                //Debug.Log("targetConnection:" + targetConnection);
                //Debug.Log("preAvatars[" + i + "]:" + "position:" + preAvatars[i].transform.position + ",eulerAngles:" + preAvatars[i].transform.eulerAngles);
                targetConnection.identity.gameObject.GetComponent<MirrorPalyer>().TargetTeleport(targetConnection, preAvatars[i].transform.position, preAvatars[i].transform.eulerAngles, x++);
                //Debug.Log("preAvatars[" + i + "]:" + "position:" + preAvatars[i].transform.position + ",eulerAngles:" + preAvatars[i].transform.eulerAngles);
            }
            else//模拟用户
            {
                GameObject simulator = index2simulatorMap[i];
                simulator.transform.position = preAvatars[i].transform.position;
                simulator.transform.eulerAngles = preAvatars[i].transform.eulerAngles;
            }

        }
    }

    public virtual void CheckValidation()
    {
        foreach(GameObject line in lines)
        {
            line.GetComponent<LineScript>().SetLineColor(Color.black);
        }
        int member_num = preAvatars.Count;
        for(int i = 0; i < member_num; i++)
        {
            bool isValid = false;
            Vector3 preAvatarPosition = preAvatars[i].transform.position;
            Collider[] colliders = Physics.OverlapSphere(new Vector3(preAvatarPosition.x, preAvatarPosition.y - offset - 0.1f, preAvatarPosition.z), 0.05f);
            if (colliders.Length != 0)
            {
                isValid = true;
            }
            if (!isValid)
            {
                lines[i].GetComponent<LineScript>().SetLineColor(Color.red);
                lines[(i - 1 + member_num) % member_num].GetComponent<LineScript>().SetLineColor(Color.red);
            }
        }
    }

/*    public Transform GetGuideTransform()
    {
        Vector3 position = preAvatars[0].transform.position - new Vector3(0, offset, 0);
        return preAvatars[0].transform;
    }*/

    public Vector3 GetGuidePosition()
    {
        return preAvatars[0].transform.position - new Vector3(0, offset - 0.8f, 0);
/*        Vector3 targetPosition = preAvatars[0].transform.position - new Vector3(0, offset - 0.8f, 0);
        Vector3 avatarPosition = index2connectionMap[0].identity.gameObject.transform.Find("Avatar").position;
        Vector3 playerPosition = index2connectionMap[0].identity.gameObject.transform.Find("Player").position;
        Vector3 lerp = avatarPosition - playerPosition;
        return targetPosition - lerp;*/
    }

    public Vector3 GetGuideEulerAngles()
    {
        Vector3 targetAngle = preAvatars[0].transform.eulerAngles;
        Vector3 avatarAngle = index2connectionMap[0].identity.gameObject.transform.Find("Avatar").eulerAngles;
        Vector3 playerAngle = index2connectionMap[0].identity.gameObject.transform.Find("Player").eulerAngles;
        Vector3 deltaAngle = new Vector3(0, avatarAngle.y - playerAngle.y, 0);
        return targetAngle - deltaAngle;
    }

    GameObject[] copiedPreAvatars;//为了记录分数
    public void RecordScore(OneTeleport oneTeleport)
    {
        StartCoroutine("RecordScoreCoroutine",oneTeleport);
    }
    IEnumerator RecordScoreCoroutine(OneTeleport oneTeleport)
    {
        //复制preAvatars，注意他们的Layer
        copiedPreAvatars = CopyPreAvatars();
        Transform exhibition;
        oneTeleport.qualities = new Quality[preAvatars.Count];
        oneTeleport.finalQualities = new double[preAvatars.Count];
        //识别目标
        //exhibition = transform.GetChild(rfFormationCnt).GetComponent<RecommendedFormation>().IsExhibition();
        exhibition = mirrorDestinationFormation.IsExhibition();
        if (exhibition)
        {
            ChangeLayer(exhibition, "Exhibition");
            //遍历所有用户
            string directory = Application.dataPath + "/Resources/Img/" + exhibition.name + "_" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
            for (int i = 0; i < preAvatars.Count; i++)
            {
                GameObject player = copiedPreAvatars[i];
                MoveCamera(player.transform);
                yield return new WaitForEndOfFrame();
                Quality quality = cameraContainer.GetComponent<VP_Quality>().getQuality();
                double finalQuality = exhibition.gameObject.GetComponent<GetFormation>().CalFinalQuality(quality);
                if (mirrorDestinationFormation.recordImg) cameraContainer.GetComponent<VP_Quality>().SaveAllDepthImg(directory, "avatar" + i + "_quality_" + finalQuality + ".png");
                oneTeleport.qualities[i] = quality;
                oneTeleport.finalQualities[i] = finalQuality;
            }
            ChangeLayer(exhibition, "Default");
            //记录
            oneTeleport.exhibition = exhibition.name;
        }
        else
        {
            //不用测量，直接记录
            oneTeleport.exhibition = "not exhibition";
        }
        //更新总记录
        string json = JsonUtility.ToJson(oneTeleport);
        File.AppendAllText(path, json + "\n");
        //销毁copy
        foreach (GameObject gameObject in copiedPreAvatars)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    void ChangeLayer(Transform trans, string targetLayer)
    {
        if (LayerMask.NameToLayer(targetLayer) == -1)
        {
            Debug.Log("Layer中不存在,请手动添加LayerName");
            return;
        }
        //遍历更改所有子物体layer
        trans.gameObject.layer = LayerMask.NameToLayer(targetLayer);
        foreach (Transform child in trans)
        {
            ChangeLayer(child, targetLayer);
        }
    }

    void MoveCamera(Transform transform)
    {
        cameraContainer.SetActive(true);
        cameraContainer.transform.rotation = Quaternion.identity;
        cameraContainer.transform.position = transform.position;
        cameraContainer.transform.forward = - new Vector3(transform.forward.x, 0, transform.forward.z);
    }

    public GameObject[] CopyPreAvatars()
    {
        GameObject[] copiedPreAvatars = new GameObject[preAvatars.Count];
        for(int i = 0; i < preAvatars.Count; i++)
        {
            GameObject copied = Instantiate(preAvatars[i].transform.Find("Head").gameObject);
            copied.transform.position = preAvatars[i].transform.Find("Head").position;
            /*            Vector3 angles = preAvatars[i].transform.Find("Head").eulerAngles;
                        copied.transform.eulerAngles = angles + new Vector3(0, 180, 0);*/
            copied.transform.forward = preAvatars[i].transform.Find("Head").forward;
            //copied.transform = preAvatars[i].transform;
            copiedPreAvatars[i] = copied;
            ChangeLayer(copied.transform, "CopyAvatar");
        }
        return copiedPreAvatars;
    }

    public void Awake()
    {
        mirrorDestinationFormation = transform.parent.gameObject.GetComponent<MirrorDestinationFormation>();
        cameraContainer = GameObject.Find("CameraContainers").transform.GetChild(0).gameObject;
        /*        string directory = Application.dataPath + "/Resources/ResultData";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                //DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss")
                path = directory + "/" + System.DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + ".json";
                File.WriteAllText(path, "----------START----------\n");*/

    }
    
    public void Start()
    {
        if (mirrorDestinationFormation.path == null)
        {
            mirrorDestinationFormation.path = Application.dataPath + "/Resources/ResultData" + "/" + System.DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + ".json";
        }
        path = mirrorDestinationFormation.path;
    }

    public void Update()
    {
        if (mirrorDestinationFormation.initialized)
        {
            UpdateFormation();
            CheckValidation();
        }
    }
}
