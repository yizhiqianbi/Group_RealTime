using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class DestinationFormation : MonoBehaviour
{
    public GameObject Followers;
    public SteamVR_Action_Boolean rotateLeftAction = SteamVR_Input.GetBooleanAction("RotateLeft");
    public SteamVR_Action_Boolean rotateRightAction = SteamVR_Input.GetBooleanAction("RotateRight");
    public SteamVR_Action_Boolean largenAction = SteamVR_Input.GetBooleanAction("Largen");
    public SteamVR_Action_Boolean lessenAction = SteamVR_Input.GetBooleanAction("Lessen");
    public SteamVR_Action_Boolean switchFormationAction = SteamVR_Input.GetBooleanAction("SwitchFormation");
    private List<GameObject> FollowerList = new List<GameObject>();
    private int formationCnt = 0;
    private GameObject NowFormation;

    [Range(1, 100)]
    public int rotateSensitivity = 50;
    [Range(1, 100)]
    public int scaleSensitivity = 50;
    // Start is called before the first frame update

    private void Awake()
    {
        int followers_num = Followers.transform.childCount;
        for (int i = 0; i < followers_num; i++)
        {
            FollowerList.Add(Followers.transform.GetChild(i).gameObject);
        }
        NowFormation = transform.GetChild(formationCnt).gameObject;
        NowFormation.SetActive(true);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (true)
        {//todo
            bool switchFormationLeft = switchFormationAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            bool switchFormationRight = switchFormationAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            if (switchFormationLeft || switchFormationRight)
            {
                SwitchFormation();
            }
        }
    }

    public void SwitchFormation()
    {
        NowFormation.SetActive(false);
        formationCnt = (formationCnt + 1) % transform.childCount;
        NowFormation = transform.GetChild(formationCnt).gameObject;
        NowFormation.SetActive(true);
    }

    public void TakeFollowers() //take followers to target positions
    {
        List<Transform> transforms = NowFormation.GetComponent<BasicFormation>().GetAllTransforms();
        for(int i = 0; i < transforms.Count - 1; i++)
        {
            FollowerList[i].transform.position = new Vector3(transforms[i + 1].position.x, FollowerList[i].transform.position.y, transforms[i + 1].position.z);
            FollowerList[i].transform.eulerAngles = transforms[i + 1].eulerAngles;
        }

    }

    public Transform GetGuideTransform() {
        return NowFormation.GetComponent<BasicFormation>().GetGuideTransform();
    }
}
