using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Valve.VR;
using System;
using System.IO;

public class MirrorDestinationFormation : NetworkBehaviour
{
    [Header("关键参数")]
    [Tooltip("用户数量")]
    public int mem_num;
    [Tooltip("是否记录数据")]
    public bool recordScore;
    [Tooltip("是否导出图片")]
    public bool recordImg;
    [Tooltip("是否显示箭头")]
    public bool showArrow;
    [Tooltip("是否显示提示")]
    public bool showGuideTips;
    [Tooltip("是否自动看向导游")]
    public bool lookAtGuide;

    [Header("非关键参数")]
    public SteamVR_Action_Boolean rotateLeftAction = SteamVR_Input.GetBooleanAction("RotateLeft");
    public SteamVR_Action_Boolean rotateRightAction = SteamVR_Input.GetBooleanAction("RotateRight");
    public SteamVR_Action_Boolean largenAction = SteamVR_Input.GetBooleanAction("Largen");
    public SteamVR_Action_Boolean lessenAction = SteamVR_Input.GetBooleanAction("Lessen");
    public SteamVR_Action_Boolean switchFormationAction = SteamVR_Input.GetBooleanAction("SwitchFormation");

    public GameObject PreAvatarPrefeb;
    public GameObject LinePrefeb;
    public GameObject GuideTips;

    public bool recommendedFormationMode;
    public bool initialized;
    private int formationCnt;
    private int rfFormationCnt;
    private GameObject NowFormation;
    public bool isTeleporting;
    public Dictionary<int, GameObject> index2simulatorMap;
    public Dictionary<int, NetworkConnection> index2connectionMap;
    public float secondFloorHeight = 3.42f;
    private List<Transform> exhibitions;
    public float timer;
    public string path;
    public void InitializeFormation()
    {
        if (!isServer)
        {
            return;
        }
        EnableExhibitions();
        GuideTips.SetActive(showGuideTips);
        index2connectionMap = new Dictionary<int, NetworkConnection>();
        int ii = 0;
        foreach (NetworkConnection connection in netIdentity.observers.Values)
        {
            index2connectionMap[ii] = connection;
            ChangeLayer(connection.identity.gameObject.transform.Find("Avatar"), "JustForShowing");
            if (ii != 0)
            {
                RemoveCrown(connection.identity.gameObject.transform);
            }
            ii++;
            connection.identity.gameObject.transform.Find("Avatar/Head/NowArrow").gameObject.SetActive(showArrow);//这个地方只在服务器设置了
        }
        //生成模拟用户
        index2simulatorMap = new Dictionary<int, GameObject>();
        for (int i = netIdentity.observers.Count; i < mem_num; i++)
        {
            GameObject simulator = Instantiate(PreAvatarPrefeb);
            simulator.transform.Find("StandPoint").gameObject.SetActive(false);
            NetworkServer.Spawn(simulator);
            Color color = GetColor(i);//指定前4个模拟用户的颜色
            simulator.GetComponent<ColorScript>().SetColor(color);

            simulator.transform.Find("Head/NowArrow").GetComponent<ArrowScript>().SetArrowColor(color);
            simulator.transform.Find("Head/NowArrow").gameObject.SetActive(showArrow);
            simulator.transform.Find("Head/PreArrow").gameObject.SetActive(false);

            ChangeLayer(simulator.transform, "JustForShowing");
            RemoveCrown(simulator.transform);
            RpcRename("simulator_" + i);
            index2simulatorMap[i] = simulator;
        }
        //将几个队形子物体初始化为不可见
        foreach (Transform child in this.transform)
        {
            child.gameObject.GetComponent<MirrorBasicFormation>().Initialize(mem_num);
        }
        NowFormation = transform.GetChild(formationCnt).gameObject;
        initialized = true;
    }

    Color GetColor(int i)//指定前4个模拟用户的颜色
    {
        Color color = Color.black;
        switch (i)
        {
            case 1:
                color = new Color(0, 0.5f, 1, 0.5f);//blue
                break;
            case 2:
                color = new Color(1, 1, 0, 0.5f);//yellow
                break;
            case 3:
                color = new Color(1, 0, 1, 0.5f);//purple
                break;
            case 4:
                color = new Color(1, 0, 0, 0.5f);//red
                break;
            default:
                color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0.5f);
                break;
        }
        return color;
    }

    [ClientRpc]
    private void RpcRename(string name)
    {
        GameObject simulator = GameObject.Find("PreAvatarPrefeb(Clone)");
        simulator.transform.name = name;
    }

    public void SetPreAvatarsActive(bool active)
    {
        if (!isServer || !initialized)
        {
            return;
        }
        //设置当前队形，并且更新isTeleporting
        NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(active);
        isTeleporting = active;//？存在时间！=决策时间
    }

    public void SwitchFormation()//判定是否正在跳转
    {
        if (!isServer || !isTeleporting)
        {
            return;
        }
/*        //变换队形
        NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(false);
        formationCnt = (formationCnt + 1) % transform.childCount;
        NowFormation = transform.GetChild(formationCnt).gameObject;
        NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(true);*/

        if (!recommendedFormationMode)
        {
            NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(false);
            formationCnt = (formationCnt + 1) % transform.childCount;
            if(formationCnt == rfFormationCnt)
            {
                formationCnt = (formationCnt + 1) % transform.childCount;
            }
            NowFormation = transform.GetChild(formationCnt).gameObject;
            NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(true);
        }
    }

/*    public Transform GetGuideTransform()
    {
        if (!isServer || !initialized)
        {
            Debug.Log("xxxxxxxxxxxxxxxxxxxx !isServer || !initialized xxxxxxxxxxxxxxxxxxxx");
            return transform;
        }
        //获取导游预览形象位置
        return NowFormation.GetComponent<MirrorBasicFormation>().GetGuideTransform();
    }*/

    public Vector3 GetGuidePosition()
    {
        if (!isServer || !initialized)
        {
            Debug.Log("xxxxxxxxxxxxxxxxxxxx !isServer || !initialized xxxxxxxxxxxxxxxxxxxx");
            return transform.position;
        }
        //获取导游预览形象位置
        return NowFormation.GetComponent<MirrorBasicFormation>().GetGuidePosition();
    }

/*    public void GetGuidePositionAfterTeleporting()
    {
        StartCoroutine("GetGuidePositionAfterTeleportingCoroutine");
    }

    IEnumerator GetGuidePositionAfterTeleportingCoroutine()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("跳转后坐标：" + index2connectionMap[0].identity.gameObject.transform.Find("Avatar").position);
    }*/

    public Vector3 GetGuideEulerAngles()
    {
        if (!isServer || !initialized)
        {
            Debug.Log("xxxxxxxxxxxxxxxxxxxx !isServer || !initialized xxxxxxxxxxxxxxxxxxxx");
            return transform.eulerAngles;
        }
        return NowFormation.GetComponent<MirrorBasicFormation>().GetGuideEulerAngles();
    }

    public void TakeFollowers()
    {
        if (!isServer || !initialized)
        {
            return;
        }
        //跳转游客，排除自身
        NowFormation.GetComponent<MirrorBasicFormation>().TakeFollowers();
    }

    public Transform IsExhibition()
    {
        foreach (Transform e in exhibitions)
        {
            if ((e.position.y < secondFloorHeight && transform.position.y < secondFloorHeight) ||
                e.position.y > secondFloorHeight && transform.position.y > secondFloorHeight)//在同一层
            {
                float ex = e.position.x;
                float ez = e.position.z;
                float x = transform.position.x;
                float z = transform.position.z;
                float r = e.gameObject.GetComponent<IdentificationRange>().range;
                if (Mathf.Pow((ex - x), 2) + Mathf.Pow((ez - z), 2) < Mathf.Pow(r, 2))//在识别区域内
                {
                    return e;
                }
            }
        }
        return null;
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

    void RemoveCrown(Transform player)
    {
        player.gameObject.GetComponent<CrownScript>().removeCrown = true;
    }

    IEnumerator EnableExhibitionsCoroutine()
    {
        yield return 0;
        foreach(Transform exhibition in exhibitions)
        {
            exhibition.gameObject.SetActive(true);
            yield return 0; yield return 0;
        }
    }

    public void EnableExhibitions()
    {
        StartCoroutine("EnableExhibitionsCoroutine");
    }


    GameObject VRCamera;
    public void SetVRCameraActive(bool active)
    {
        if (!VRCamera) VRCamera = index2connectionMap[0].identity.gameObject.transform.Find("Player/SteamVRObjects/VRCamera").gameObject;
        VRCamera.SetActive(active);
    }

    public void Awake()
    {
        //preAvatars = new List<GameObject>();
        initialized = false;
        isTeleporting = false;
        recommendedFormationMode = false;
        formationCnt = 0;
        //获取rfFormationCnt
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.CompareTag("RecommendedFormation"))
            {
                rfFormationCnt = i;
                break;
            }
        }
        //获取所有展品
        exhibitions = new List<Transform>();
        Transform mySceneTrans = GameObject.Find("MyScene").transform;
        for (int i = 0; i < mySceneTrans.childCount; i++)
        {
            exhibitions.Add(mySceneTrans.GetChild(i));
        }
        //数据记录
        string directory = Application.dataPath + "/Resources/ResultData";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        //DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss")
        path = directory + "/" + System.DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + ".json";
        File.WriteAllText(path, "----------START----------\n");
    }

    public void Update()
    {
        bool switchFormationLeft = switchFormationAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
        bool switchFormationRight = switchFormationAction.GetStateDown(SteamVR_Input_Sources.RightHand);
        if (switchFormationLeft || switchFormationRight)
        {
            if (!initialized)
            {
                InitializeFormation();
            }
            else
            {
                SwitchFormation();
            } 
        }
        if (initialized && !isTeleporting)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                recommendedFormationMode = !recommendedFormationMode;
                if (recommendedFormationMode)
                {
                    NowFormation = transform.GetChild(rfFormationCnt).gameObject;
                }
                else
                {
                    NowFormation = transform.GetChild(formationCnt).gameObject;
                }
                
            }
        }
        /*        if (isTeleporting)
                {
                    NowFormation.GetComponent<MirrorBasicFormation>().UpdateGuideBezier();
                }*/

        /*        if (Input.GetKeyDown(KeyCode.I))
                {
                    InitializeFormation();
                }else if (Input.GetKeyDown(KeyCode.S))
                {
                    SwitchFormation();
                }else if (Input.GetKeyDown(KeyCode.A))
                {
                    NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(true);
                }else if (Input.GetKeyDown(KeyCode.T))
                {
                    NowFormation.GetComponent<MirrorBasicFormation>().TakeFollowers();
                    NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(false);
                }*/
        if (isTeleporting)
        {
            timer += Time.deltaTime;
        }
    }
}
