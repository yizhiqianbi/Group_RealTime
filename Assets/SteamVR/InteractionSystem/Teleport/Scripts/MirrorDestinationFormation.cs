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
    [Header("�ؼ�����")]
    [Tooltip("�û�����")]
    public int mem_num;
    [Tooltip("�Ƿ��¼����")]
    public bool recordScore;
    [Tooltip("�Ƿ񵼳�ͼƬ")]
    public bool recordImg;
    [Tooltip("�Ƿ���ʾ��ͷ")]
    public bool showArrow;
    [Tooltip("�Ƿ���ʾ��ʾ")]
    public bool showGuideTips;
    [Tooltip("�Ƿ��Զ�������")]
    public bool lookAtGuide;

    [Header("�ǹؼ�����")]
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
            connection.identity.gameObject.transform.Find("Avatar/Head/NowArrow").gameObject.SetActive(showArrow);//����ط�ֻ�ڷ�����������
        }
        //����ģ���û�
        index2simulatorMap = new Dictionary<int, GameObject>();
        for (int i = netIdentity.observers.Count; i < mem_num; i++)
        {
            GameObject simulator = Instantiate(PreAvatarPrefeb);
            simulator.transform.Find("StandPoint").gameObject.SetActive(false);
            NetworkServer.Spawn(simulator);
            Color color = GetColor(i);//ָ��ǰ4��ģ���û�����ɫ
            simulator.GetComponent<ColorScript>().SetColor(color);

            simulator.transform.Find("Head/NowArrow").GetComponent<ArrowScript>().SetArrowColor(color);
            simulator.transform.Find("Head/NowArrow").gameObject.SetActive(showArrow);
            simulator.transform.Find("Head/PreArrow").gameObject.SetActive(false);

            ChangeLayer(simulator.transform, "JustForShowing");
            RemoveCrown(simulator.transform);
            RpcRename("simulator_" + i);
            index2simulatorMap[i] = simulator;
        }
        //�����������������ʼ��Ϊ���ɼ�
        foreach (Transform child in this.transform)
        {
            child.gameObject.GetComponent<MirrorBasicFormation>().Initialize(mem_num);
        }
        NowFormation = transform.GetChild(formationCnt).gameObject;
        initialized = true;
    }

    Color GetColor(int i)//ָ��ǰ4��ģ���û�����ɫ
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
        //���õ�ǰ���Σ����Ҹ���isTeleporting
        NowFormation.GetComponent<MirrorBasicFormation>().SetPreAvatarsActive(active);
        isTeleporting = active;//������ʱ�䣡=����ʱ��
    }

    public void SwitchFormation()//�ж��Ƿ�������ת
    {
        if (!isServer || !isTeleporting)
        {
            return;
        }
/*        //�任����
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
        //��ȡ����Ԥ������λ��
        return NowFormation.GetComponent<MirrorBasicFormation>().GetGuideTransform();
    }*/

    public Vector3 GetGuidePosition()
    {
        if (!isServer || !initialized)
        {
            Debug.Log("xxxxxxxxxxxxxxxxxxxx !isServer || !initialized xxxxxxxxxxxxxxxxxxxx");
            return transform.position;
        }
        //��ȡ����Ԥ������λ��
        return NowFormation.GetComponent<MirrorBasicFormation>().GetGuidePosition();
    }

/*    public void GetGuidePositionAfterTeleporting()
    {
        StartCoroutine("GetGuidePositionAfterTeleportingCoroutine");
    }

    IEnumerator GetGuidePositionAfterTeleportingCoroutine()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("��ת�����꣺" + index2connectionMap[0].identity.gameObject.transform.Find("Avatar").position);
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
        //��ת�οͣ��ų�����
        NowFormation.GetComponent<MirrorBasicFormation>().TakeFollowers();
    }

    public Transform IsExhibition()
    {
        foreach (Transform e in exhibitions)
        {
            if ((e.position.y < secondFloorHeight && transform.position.y < secondFloorHeight) ||
                e.position.y > secondFloorHeight && transform.position.y > secondFloorHeight)//��ͬһ��
            {
                float ex = e.position.x;
                float ez = e.position.z;
                float x = transform.position.x;
                float z = transform.position.z;
                float r = e.gameObject.GetComponent<IdentificationRange>().range;
                if (Mathf.Pow((ex - x), 2) + Mathf.Pow((ez - z), 2) < Mathf.Pow(r, 2))//��ʶ��������
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
            Debug.Log("Layer�в�����,���ֶ����LayerName");
            return;
        }
        //������������������layer
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
        //��ȡrfFormationCnt
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.CompareTag("RecommendedFormation"))
            {
                rfFormationCnt = i;
                break;
            }
        }
        //��ȡ����չƷ
        exhibitions = new List<Transform>();
        Transform mySceneTrans = GameObject.Find("MyScene").transform;
        for (int i = 0; i < mySceneTrans.childCount; i++)
        {
            exhibitions.Add(mySceneTrans.GetChild(i));
        }
        //���ݼ�¼
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
