using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

class Avatar
{
    public GameObject obj;
    public Preference pre = Preference.Normal;
    public int preI;
    public int preJ;
    public int i;
    public int j;
    public int nextI;
    public int nextJ;
}

class Point
{
    public int i;
    public int j;
    public bool isValid;
    public bool takePicture;

    public Point()
    {

    }

    public Point(int i,int j,bool takePicture)//���ӻ�ʱ����
    {
        this.i = i;
        this.j = j;
        this.takePicture = takePicture;
    }
    public void setValue(int i, int j, bool isValid)
    {
        this.i = i;
        this.j = j;
        this.isValid = isValid;
    }
}

public class GetFormation : MonoBehaviour
{
    [Header("������ʾ����-��չʾ")]
    public bool isGettingMaxQualiies;

    [Header("������ز���")]
    [Tooltip("�ֲ��Ƕȣ�ԽСԽ��")]
    public float angleSum = 360;
    [Tooltip("�����ܶ�")]
    public float gridSpacing = 0.1f;
    [Tooltip("�����뾶")]
    public float far = 4;
    [Tooltip("������֮�����С���룬���ڱ�����ײ")]
    public float minDistance = 0.5f;
    [Tooltip("����������")]
    public int maxMeasuringTime = 60;

    [Header("����ϵ��")]
    [Range(0, 100)]
    public float k_size = 40;
    [Range(0, 100)]
    public float k_depth = 20;
    [Range(0, 100)]
    public float k_color = 80;
    [Range(0, 100)]
    public float k_covered = 100;
    [Range(0, 100)]
    public float k_integrity = 100;

    [Header("��������-��չʾ")]
    public double sizeQuality;
    public double depthQuality;
    public double colorfulnessQuality;
    public double coveredQuality;
    public double integrityQuality;
    public double finalQuality;
    public double maxColorfulnessQuality;
    public double maxDepthQuality;
    public double maxSizeQuality;

    [Header("����")]
    public int test_num;
    public bool test;
    private bool testFlag;
    [Tooltip("չʾ���вι۵�")]
    public bool checkPositions;
    private List<Tuple<Transform, Preference>> testInputAvatars;
    public bool showAllScores;
    public bool showTrace;
    public bool minAngle;
    [Tooltip("ֻ���س�ʼ����")]
    public bool IRF;

    private List<Avatar> avatars;
    private bool isGettingRF;
    private bool isGettingInitialRF;
    private Dictionary<int, List<Tuple<int, int>>> initialRFs;//��Ҫ��֤��ƫת�Ƕ���������
    private Dictionary<int, Dictionary<int, List<Vector3>>> RFs;//��һ��int��ʾ�������ڶ�����ʾ�����Ƶ�����ת����List<Preference>

    private GameObject viewpoints;
    private GameObject avatarModel_Simple;
    private GameObject avatarModel;
    private GameObject avatarModel_Close;
    private GameObject avatarModel_Far;
    private CameraContainers camerasSyn;
    private List<GameObject> cameraContainers;
    private GameObject traces;
    private bool[,] validList = null;
    private bool[,] nextList = null;
    private bool[,] nowList = null;
    private int line;
    private float height;//�û�����ĸ߶�
    private float floorHeight;//�û������ڲ�ذ�ĸ߶�

    Stopwatch sw;

    public void Awake()
    {
        if (transform.position.y > 3.42f)
        {
            height = 5.16337f;
            floorHeight = 3.42f;
        }
        else
        {
            height = 2.03003f;
            floorHeight = 0.33f;
        }
        maxColorfulnessQuality = maxDepthQuality = maxSizeQuality = 0;
        isGettingRF = false;
        isGettingInitialRF = false;
        isGettingMaxQualiies = false;
        test = testFlag = false;
        showAllScores = false;
        showTrace = false;
        minAngle = true;
        initialRFs = new Dictionary<int, List<Tuple<int, int>>>();
        RFs = new Dictionary<int, Dictionary<int, List<Vector3>>>();
        avatars = new List<Avatar>();
        avatarModel_Simple = GameObject.Find("FollowerModel_Simple");
        avatarModel = GameObject.Find("FollowerModel");
        avatarModel_Close = GameObject.Find("FollowerModel_Close");
        avatarModel_Far = GameObject.Find("FollowerModel_Far");
        camerasSyn = GameObject.Find("CameraContainers").GetComponent<CameraContainers>(); 
        cameraContainers = new List<GameObject>();
        GameObject parent = GameObject.Find("CameraContainers");
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            cameraContainers.Add(child.gameObject);
        }
        traces = new GameObject("traces");
        traces.transform.SetParent(transform);//��ʾ�켣

        GetValidPositions();
        StartCoroutine("GetMaxQualiies");//��ȡ�������ֵ

        //test
        sw = new Stopwatch();
        testInputAvatars = new List<Tuple<Transform, Preference>>();
        for (int i = 0; i < test_num; i++)
        {
            testInputAvatars.Add(new Tuple<Transform, Preference>(transform, Preference.Close));
        }
    }

    Dictionary<int, GameObject> redDots;

    void GetValidPositions()
    {
        redDots = new Dictionary<int, GameObject>();
        viewpoints = new GameObject("viewpoints");
        viewpoints.transform.SetParent(transform);//�ӵ���ӻ�
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        Vector2 center = new Vector2(center_x, center_z);
        line = Mathf.FloorToInt(2 * far / gridSpacing) + 1;
        validList = new bool[line, line];
        nextList = new bool[line, line];
        nowList = new bool[line, line];
        for (int i = 0; i < line; i++)
        {
            for (int j = 0; j < line; j++)
            {
                float x = center_x - far + gridSpacing * i;
                float z = center_z - far + gridSpacing * j;
                Vector2 point = new Vector2(x, z);
                float distance = Vector2.Distance(center, point);
                if (distance <= far)
                {
                    Collider[] colliders = Physics.OverlapSphere(new Vector3(x, height, z), 0.05f);
                    Collider[] colliders1 = Physics.OverlapSphere(new Vector3(x, floorHeight - 0.1f, z), 0.05f);
                    if (colliders.Length == 0 && colliders1.Length != 0)
                    {
                        validList[i, j] = true;
                        //�ӵ���ӻ�
                        if (checkPositions)
                        {
                            GameObject redDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            redDot.GetComponent<Collider>().enabled = false;
                            redDot.transform.SetParent(viewpoints.transform);
                            redDot.transform.position = new Vector3(x, height, z);
                            redDot.GetComponent<Renderer>().material.color = Color.blue;
                            redDot.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                            redDots[i * line + j] = redDot;
                        }
                    }
                    else
                    {
                        validList[i, j] = false;
                    }
                }
                else
                {
                    validList[i, j] = false;
                }
                nextList[i, j] = false;
                nowList[i, j] = false;
            }
        }
    }

    IEnumerator GetMaxQualiies()
    {
        //�ڽ�����9�������һ�����ֵ
        isGettingMaxQualiies = true;
        ChangeLayer(this.transform, "Exhibition");
        List<Point> points = new List<Point>();
        for (int i = 0; i < 9; i++)
        {
            float angle = 40 * i;
            float r = 0;
            Point point = new Point();
            point.takePicture = false;
            while (r < far)
            {
                r += gridSpacing;
                float x = r * Mathf.Cos(Mathf.Deg2Rad * angle);
                float z = r * Mathf.Sin(Mathf.Deg2Rad * angle);
                int point_i = Mathf.FloorToInt((x + far) / gridSpacing);
                int point_j = Mathf.FloorToInt((z + far) / gridSpacing);
                if (isValid(point_i, point_j))
                {
                    point.i = point_i;
                    point.j = point_j;
                    point.takePicture = true;
                    break;
                }
            }
            points.Add(point);
            SetCamera(point, i);
        }
        yield return new WaitForEndOfFrame();
        string directory = Application.dataPath + "/Resources/MaxQ/" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
        for (int i = 0; i < points.Count; i++)
        {
            GameObject cameraContainer = cameraContainers[i];
            if (points[i].takePicture)
            {
                //cameraContainer.GetComponent<VP_Quality>().SaveAllDepthImg(directory, "point_" + i + ".png");
                Quality quality = cameraContainer.GetComponent<VP_Quality>().getQuality();
                UpdateQuality(quality, true);//��������Լ��������ֵ
            }
        }
        DisableCameras();
        ChangeLayer(this.transform, "Default");
        isGettingMaxQualiies = false;
    }

    float pre2dis(Preference preference)
    {
        switch (preference)
        {
            case Preference.Close:
                return 0.5f;
            case Preference.Normal:
                return 0.75f;
            case Preference.Far:
                return 1;
        }
        return 1;
    }

    void UpdateDots(Avatar avatar)
    {
        //��Χ�ڱ��
        float dis = pre2dis(avatar.pre);
        int x = Mathf.FloorToInt(dis / gridSpacing);
        for(int i = -x - 2; i< x + 2; i++)
        {
            for(int j = -x-2; j< x+2; j++)
            {
                int ii = i + avatar.i;
                int jj = j + avatar.j;
                if (isValid(ii, jj) && Mathf.Sqrt(Mathf.Pow(i * gridSpacing, 2) + Mathf.Pow(j * gridSpacing, 2)) <= dis)
                {
                    redDots[ii * line + jj].GetComponent<MeshRenderer>().material.color = Color.red;
                }
            }
        }
    }

    //����IRF���ɶ�����ʼ���Σ�����avatars
    public void InitializeAvatarsFromIRF(Tuple<List<Tuple<int, Vector3>>, List<Tuple<Transform, Preference>>> rawIRFandInputAvatars)
    {
        List<Tuple<int, Vector3>> rawIRF = rawIRFandInputAvatars.Item1;
        List<Tuple<Transform, Preference>> inputAvatars = rawIRFandInputAvatars.Item2;
        avatars.Clear();//�Է���һ
        int n = rawIRF.Count;
        for(int index = 0; index < n; index++)
        {
            Vector3 position = rawIRF[index].Item2;
            Preference preference = inputAvatars[rawIRF[index].Item1].Item2;
            Tuple<int, int> ij = position2ij(position);
            int i = ij.Item1;
            int j = ij.Item2;
            nowList[i, j] = true;
            Avatar avatar = new Avatar();
            avatar.preI = avatar.i = i;
            avatar.preJ = avatar.j = j;
            GameObject avatarObj = GameObject.Instantiate(avatarModel_Far);
            switch (preference)
            {
                case Preference.Close:
                    avatarObj = GameObject.Instantiate(avatarModel_Close);
                    break;
                case Preference.Normal:
                    avatarObj = GameObject.Instantiate(avatarModel);
                    break;
                case Preference.Far:
                    avatarObj = GameObject.Instantiate(avatarModel_Far);
                    break;
            }
            avatarObj.transform.position = ij2position(i, j);
            Vector3 forward = this.transform.position - avatarObj.transform.position;
            avatarObj.transform.forward = new Vector3(forward.x, 0, forward.z);
            ChangeLayer(avatarObj.transform, "CopyAvatar");
            avatar.obj = avatarObj;
            avatar.pre = preference;
            avatars.Add(avatar);
        }
    }


    //��������n���ɳ�ʼ���Σ�����avatars
    public void InitializeAvatars(int n)
    {
        avatars.Clear();//�Է���һ
        float angle = angleSum / n;
        float now_angle = 0;
        for (int avatar_i = 0; avatar_i < n; avatar_i++)
        {
            float r = far;
            now_angle += angle;
            while (true)
            {
                //Debug.Log("isGeneratingAvatars");
                r -= gridSpacing;
                if (r < gridSpacing)
                {
                    r = far;
                    now_angle += angle;
                }
                float x = r * Mathf.Cos(Mathf.Deg2Rad * now_angle);
                float z = r * Mathf.Sin(Mathf.Deg2Rad * now_angle);
                int i = Mathf.FloorToInt((x + far) / gridSpacing);
                int j = Mathf.FloorToInt((z + far) / gridSpacing);
                if (isValid(i, j) && !nowList[i, j])
                {
                    bool tooClose = false;
                    //�ж��ǲ���̫��
                    for (int ii = 0; ii < avatar_i; ii++)
                    {
                        Vector3 position = ij2position(i, j);
                        Vector3 other_position = ij2position(avatars[ii].i, avatars[ii].j);
                        double dis = Math.Sqrt(Math.Pow(position.x - other_position.x, 2) + Math.Pow(position.z - other_position.z, 2));
                        if (dis < pre2dis(Preference.Far))
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (!tooClose)
                    {
                        nowList[i, j] = true;
                        Avatar avatar = new Avatar();
                        avatar.preI = avatar.i = i;
                        avatar.preJ = avatar.j = j;
                        //GameObject avatarObj = GameObject.Instantiate(avatarModel);
                        GameObject avatarObj = GameObject.Instantiate(avatarModel_Far);
                        //GameObject avatarObj = GameObject.Instantiate(avatarModel_Simple);
                        ChangeLayer(avatarObj.transform, "CopyAvatar");
                        avatarObj.transform.position = ij2position(i, j);
                        Vector3 forward = this.transform.position - avatarObj.transform.position;
                        avatarObj.transform.forward = new Vector3(forward.x, 0, forward.z);
                        avatar.obj = avatarObj;
                        //��ͼ
/*                        switch (avatar_i)
                        {
                            case 0:
                                avatar.pre = Preference.Close;
                                break;
                            case 1:
                                avatar.pre = Preference.Close;
                                break;
                            case 2:
                                avatar.pre = Preference.Close;
                                break;
                            default:
                                avatar.pre = Preference.Far;
                                break;
                        }*/
                        //����
                        avatar.pre = Preference.Far;
                        avatars.Add(avatar);
                        if(checkPositions) UpdateDots(avatar);
                        break;
                    }
                }
            }
        }
    }

    //������һ��ʹ�õ��û������ˣ���������nowList��nextList
    void Restore()
    {
        foreach (Avatar a in avatars)//����avatars��������Ϸ����
        {
            Destroy(a.obj);
        }
        avatars.Clear();
        for (int i = 0; i < line; i++)
        {
            for (int j = 0; j < line; j++)
            {
                nowList[i, j] = nextList[i, j] = false;
            }
        }
    }

    void ChangeLayer(Transform trans, string targetLayer)
    {
        if (LayerMask.NameToLayer(targetLayer) == -1)
        {
            //UnityEngine.Debug.Log("Layer�в�����,���ֶ�����LayerName");
            return;
        }
        //������������������layer
        trans.gameObject.layer = LayerMask.NameToLayer(targetLayer);
        foreach (Transform child in trans)
        {
            ChangeLayer(child, targetLayer);
        }
    }

    //�жϸ�λ���Ƿ���Ч���ڷ�Χ�����ڿ�����������
    bool isValid(int i, int j)
    {
        if (i < 0 || i >= line || j < 0 || j >= line)
        {
            return false;
        }
        return validList[i, j];
    }

    bool TooClose(int index, int ii, int jj)
    {
        Vector3 position = ij2position(ii, jj);
        for (int i = 0; i < avatars.Count; i++)
        {
            if (i != index)
            {
                Vector3 other_position = ij2position(avatars[i].i, avatars[i].j);
                double dis = Math.Sqrt(Math.Pow(position.x - other_position.x, 2) + Math.Pow(position.z - other_position.z, 2));
                if (dis < Math.Max(pre2dis(avatars[i].pre), pre2dis(avatars[index].pre)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    Point genPoint(int i, int j)
    {
        Point point = new Point();
        point.setValue(i, j, isValid(i, j));
        return point;
    }

    List<Point> generatePoints(int i, int j)
    {
        List<Point> points = new List<Point>();
        points.Add(genPoint(i, j));
        points.Add(genPoint(i - 1, j - 1));
        points.Add(genPoint(i, j - 1));
        points.Add(genPoint(i + 1, j - 1));
        points.Add(genPoint(i - 1, j));
        points.Add(genPoint(i + 1, j));
        points.Add(genPoint(i - 1, j + 1));
        points.Add(genPoint(i, j + 1));
        points.Add(genPoint(i + 1, j + 1));
        return points;
    }

    Vector3 ij2position(int i, int j)
    {
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        float x = center_x - far + gridSpacing * i;
        float z = center_z - far + gridSpacing * j;
        return new Vector3(x, height, z);
    }

    Tuple<int, int> position2ij(Vector3 position)
    {
        float x = position.x;
        float z = position.z;
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        float delta_x = x - (center_x - far) + gridSpacing * 0.01f;
        float delta_z = z - (center_z - far) + gridSpacing * 0.01f;
        int i = (int)(delta_x / gridSpacing);
        int j = (int)(delta_z / gridSpacing);
        return new Tuple<int, int>(i, j);
    }

    public double CalFinalQuality(Quality quality)
    {
        double k_sum = k_size + k_integrity + k_color + k_depth + k_covered;
        integrityQuality = quality.integrity_quality;
        colorfulnessQuality = quality.colorfulness_quality / maxColorfulnessQuality;
        depthQuality = quality.depth_quality / maxDepthQuality;
        sizeQuality = quality.size_quality / maxSizeQuality;
        coveredQuality = quality.covered_quality;
        finalQuality = (k_size / k_sum) * sizeQuality + (k_integrity / k_sum) * integrityQuality +
            (k_color / k_sum) * colorfulnessQuality + (k_depth / k_sum) * depthQuality +
            (k_covered / k_sum) * coveredQuality;
        return finalQuality;
    }

    Dictionary<Avatar, List<Vector3>> avatar2traceDic;
    void RecordTrace(Avatar avatar, Vector3 nowPosition, Vector3 nextPosition)
    {
        if (avatar2traceDic == null) avatar2traceDic = new Dictionary<Avatar, List<Vector3>>();
        if (!avatar2traceDic.ContainsKey(avatar))
        {
            avatar2traceDic[avatar] = new List<Vector3>();
            avatar2traceDic[avatar].Add(new Vector3(nowPosition.x, floorHeight + 0.05f, nowPosition.z));
            avatar2traceDic[avatar].Add(new Vector3(nextPosition.x, floorHeight + 0.05f, nextPosition.z));
        }
        else
        {
            avatar2traceDic[avatar].Add(new Vector3(nextPosition.x, floorHeight + 0.05f, nextPosition.z));
        }
    }

    void ShowTrace()
    {
        foreach(Avatar avatar in avatar2traceDic.Keys)
        {
            var tempColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);
            Material traceLineMaterial = new Material(Shader.Find("Standard"));
            traceLineMaterial.color = tempColor;
            avatar.obj.transform.Find("Shirt").GetComponent<MeshRenderer>().material = traceLineMaterial;
            List<Vector3> positions = avatar2traceDic[avatar];
            GameObject oneTrace = new GameObject();
            oneTrace.transform.SetParent(traces.transform);
            LineRenderer line = oneTrace.AddComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.045f;
            line.positionCount = positions.Count;
            line.SetPositions(positions.ToArray());
            line.material = traceLineMaterial;
        }
    }

    bool GotoNextPositions()
    {
        bool oscillating = true;
        foreach (Avatar avatar in avatars)
        {
            nextList[avatar.nextI, avatar.nextJ] = false;
            nowList[avatar.i, avatar.j] = false;
            nowList[avatar.nextI, avatar.nextJ] = true;
            if (!(avatar.preI == avatar.nextI && avatar.preJ == avatar.nextJ))
            {
                oscillating = false;
            }
            avatar.preI = avatar.i;
            avatar.preJ = avatar.j;
            if (!(avatar.i == avatar.nextI && avatar.j == avatar.nextJ))
            {
                //Debug.Log("(" + avatar.i + "," + avatar.j + ")->(" + avatar.nextI + "," + avatar.nextJ + ")");
                Vector3 nextPosition = ij2position(avatar.nextI, avatar.nextJ);
                if (showTrace) RecordTrace(avatar, avatar.obj.transform.position, nextPosition);
                avatar.obj.transform.position = nextPosition;
                Vector3 forward = this.transform.position - avatar.obj.transform.position;
                avatar.obj.transform.forward = new Vector3(forward.x, 0, forward.z);
                avatar.i = avatar.nextI;
                avatar.j = avatar.nextJ;
            }
        }
        return !oscillating;
    }

    void SetCamera(Point point, int i)//���õ�i�������״̬����Ч���ƶ�����Ч��ʧ��
    {
        GameObject cameraContainer = cameraContainers[i];
        if (!point.takePicture)
        {
            cameraContainer.SetActive(false);
            return;
        }
        cameraContainer.SetActive(true);
        cameraContainer.transform.rotation = Quaternion.identity;
        cameraContainer.transform.position = ij2position(point.i, point.j);
        Vector3 forward = transform.position - cameraContainer.transform.position;
        cameraContainer.transform.forward = new Vector3(forward.x, 0, forward.z);
    }

    void DisableCameras()
    {
        foreach(GameObject cameraContainer in cameraContainers)
        {
            cameraContainer.SetActive(false);
        }
    }

    MirrorDestinationFormation mirrorDestinationFormation;
    void SetVRCameraActive(bool active)
    {
        if (!mirrorDestinationFormation) mirrorDestinationFormation = GameObject.Find("MirrorDestinationFormation").GetComponent<MirrorDestinationFormation>();
        mirrorDestinationFormation.SetVRCameraActive(active);
    }

    void UpdateQuality(Quality quality, bool pre)
    {
        double k_sum = k_size + k_integrity + k_color + k_depth + k_covered;
        integrityQuality = quality.integrity_quality;
        coveredQuality = quality.covered_quality;
        if (pre)
        {
            colorfulnessQuality = quality.colorfulness_quality;
            depthQuality = quality.depth_quality;
            sizeQuality = quality.size_quality;
            if (colorfulnessQuality > maxColorfulnessQuality) maxColorfulnessQuality = colorfulnessQuality;
            if (depthQuality > maxDepthQuality) maxDepthQuality = depthQuality;
            if (sizeQuality > maxSizeQuality) maxSizeQuality = sizeQuality;
        }
        else
        {
            colorfulnessQuality = quality.colorfulness_quality / maxColorfulnessQuality;
            depthQuality = quality.depth_quality / maxDepthQuality;
            sizeQuality = quality.size_quality / maxSizeQuality;
        }
        finalQuality = (k_size / k_sum) * sizeQuality + (k_integrity / k_sum) * integrityQuality +
            (k_color / k_sum) * colorfulnessQuality + (k_depth / k_sum) * depthQuality +
            (k_covered / k_sum) * coveredQuality;
    }

    IEnumerator GetBalanced()
    {//avatars���ǳ�ʼ����
        //SetVRCameraActive(false);
        for (int measuringTime = 1; measuringTime <= maxMeasuringTime; measuringTime++)
        {
            //Debug.Log("---------------------[measureTime=" + measuringTime + "]---------------------");
            for (int index = 0; index < avatars.Count; index++)
            {
                Avatar avatar = avatars[index];
                ChangeLayer(avatar.obj.transform, "JustForShowing");
                avatar.obj.transform.Find("Flag").gameObject.SetActive(true);
                List<Point> points = generatePoints(avatar.i, avatar.j);
                for(int i = 0; i < points.Count; i++)
                {
                    Point point = points[i];
                    //��Ч�Ĳ���û�˲���û��ȥ�����
                    point.takePicture = isValid(point.i, point.j) && !nextList[point.i, point.j] && (!nowList[point.i, point.j] || i == 0) && (!TooClose(index, point.i, point.j) || i == 0);
                    SetCamera(point, i);//���õ�i�������״̬����Ч���ƶ�����Ч��ʧ��
                }
                yield return new WaitForEndOfFrame();//�ȴ����������Ⱦ���֮����ȥ�������
                //UnityEngine.Debug.Log("Hello I am Frame End");
                double maxQuality = -100;
                for (int i = 0; i < points.Count; i++)//������Χ�ĵ㣬Ѱ�ҷ�����ߵĵ�
                {
                    Point point = points[i];
                    GameObject cameraContainer = cameraContainers[i];
                    if (point.takePicture)
                    {
                        Quality quality = cameraContainer.GetComponent<VP_Quality>().getQuality();
                        UpdateQuality(quality, false);//��������Լ������ܷ���finalQuality
                        if (i == 0)
                        {
                            maxQuality = finalQuality;
                            //Debug.Log("[avatar" + index + "] initial state:([" + point.i + "," + point.j + ")," + finalQuality + "]");
                            avatar.nextI = point.i;
                            avatar.nextJ = point.j;
                        }
                        else
                        {
                            if (maxQuality < finalQuality)
                            {
                                //Debug.Log("[avatar" + index + "] updateMax:[(" + avatar.nextI + "," + avatar.nextJ + ")," + maxQuality + "] -> [(" + point.i + "," + point.j + "),quality:" + finalQuality + "]");
                                maxQuality = finalQuality;
                                avatar.nextI = point.i;
                                avatar.nextJ = point.j;
                            }
                        }
                    }
                }
                nextList[avatar.nextI, avatar.nextJ] = true;//����һ����ǣ������˲���ȥ
                ChangeLayer(avatar.obj.transform, "CopyAvatar");
                avatar.obj.transform.Find("Flag").gameObject.SetActive(false);
            }
            bool go = GotoNextPositions();
            //Debug.Log("go=" + go);
            if (!go)
            {
                sw.Stop();
                //UnityEngine.Debug.Log("ʱ��: " + sw.ElapsedMilliseconds + " ����");
                //UnityEngine.Debug.Log("FINISH: �����ִ�: " + measuringTime);
                DisableCameras();
                //SetVRCameraActive(true);
                break;
            }     
        }
        if (showTrace) ShowTrace();
    }

    double forward2angle(Vector3 forward)
    {
        //Ĭ����z��֮��ļнǣ�0-360��
        double angle = Math.Atan2(forward.x, forward.z) * 180 / Math.PI;
        if (angle < 0) angle += 360;
        return angle;
    }

    double deltaAngle(double a, double b)
    {
        double delta = Math.Abs(a - b);
        if(delta <= 180)
        {
            return delta;
        }
        else
        {
            return 360 - delta;
        }
    }

    //��¼avatars��λ�����ݵ�initialRFs[i]�У����ճ�����������
    void RecordIRF()
    {
        SortedList<double, Tuple<int, int>> sortedIRF = new SortedList<double, Tuple<int, int>>();
        for(int i = 0; i < avatars.Count; i++)
        {
            Avatar avatar = avatars[i];
            Vector3 position = ij2position(avatar.i, avatar.j);
            Vector3 temp = transform.position - position;
            Vector3 forward = new Vector3(temp.x, 0, temp.z);
            sortedIRF.Add(forward2angle(forward), new Tuple<int, int>(avatar.i, avatar.j));
        }
        List<Tuple<int, int>> initialRF = new List<Tuple<int, int>>();
        foreach(var key in sortedIRF.Keys)
        {
            initialRF.Add(sortedIRF[key]);
        }
        initialRFs.Add(avatars.Count, initialRF);
    }

    //��¼avatars��λ�õ�rawIRF��Ӧ˳���pre������
    void RecordRF(Tuple<List<Tuple<int, Vector3>>, List<Tuple<Transform, Preference>>> rawIRFandInputAvatars)
    {
        List<Tuple<int, Vector3>> rawIRF = rawIRFandInputAvatars.Item1;
        List<Tuple<Transform, Preference>> inputAvatars = rawIRFandInputAvatars.Item2;
        List<Vector3> positions = new List<Vector3>();
        List<Preference> preferences = new List<Preference>();
        int n = rawIRF.Count;
        for (int i = 0; i < n; i++)
        {
            positions.Add(ij2position(avatars[i].i, avatars[i].j));
            preferences.Add(inputAvatars[rawIRF[i].Item1].Item2);
        }
        if (!RFs.ContainsKey(n)) RFs.Add(n, new Dictionary<int, List<Vector3>>());
        RFs[n].Add(preferences2int(preferences), positions);
    }

    //rawIRF����
    IEnumerator GetRF(Tuple<List<Tuple<int, Vector3>>, List<Tuple<Transform, Preference>>> rawIRFandInputAvatars)
    {
        if (camerasSyn.busy) yield break;
        camerasSyn.busy = true;
        isGettingRF = true;
        ChangeLayer(this.transform, "Exhibition");

        //1.������ʼ����
        InitializeAvatarsFromIRF(rawIRFandInputAvatars);
        //2.�����õ������Ƽ�����
        yield return StartCoroutine("GetBalanced");
        //3.��¼����
        RecordRF(rawIRFandInputAvatars);
        //4.�ָ���ʼ״̬
        Restore();
        
        ChangeLayer(this.transform, "Default");
        isGettingRF = false;
        camerasSyn.busy = false;
    }

    //������������Ԥ�Ƽ����Σ����ں�������
    //����initialRFs[i]
    IEnumerator GetInitialRF(int n)
    {
        if (camerasSyn.busy) yield break;
        camerasSyn.busy = true;
        isGettingInitialRF = true;
        ChangeLayer(this.transform, "Exhibition");
        //1.��ʼ���Σ����ȡ�����ײ
        InitializeAvatars(n);
        if (!IRF) {
            //2.�����õ�Ԥ�Ƽ����Σ�һ���Ƿֶ�ִ֡��
            yield return StartCoroutine("GetBalanced");
        }
        //3.��¼���ݣ����ճ�����������
        RecordIRF();
        //4.�ָ���ʼ״̬
        Restore();
        //Ӧ����Ϊ���ڵ��Ե�ʱ���ܹ�����Ч����
        ChangeLayer(this.transform, "Default");
        isGettingInitialRF = false;
        camerasSyn.busy = false;
    }

    //���ظ�����Сƫת��ƥ���Ԥ�Ƽ����Σ�int��ʾӳ���ϵ��inputAvatars[List[i].Item1]��ӦList[i].Item2
    public List<Tuple<int, Vector3>> MatchWithInitialRF(List<Tuple<Transform, Preference>> visitors)
    {
        List<Tuple<int, int>> initialRF = initialRFs[visitors.Count];
        List<Tuple<int, Vector3>> matchedIRF = new List<Tuple<int, Vector3>>();
        //ƥ����Сƫת��
        //TODO
        SortedList<double, int> sortedVisitors = new SortedList<double, int>();//double->angle int->index
        for(int index = 0; index < visitors.Count; index++)
        {
            double angle = forward2angle(visitors[index].Item1.forward);
            while (sortedVisitors.ContainsKey(angle))angle += 0.000001f;
            sortedVisitors.Add(angle, index);
        }
        List<double> initialAngles = new List<double>();
        foreach(var avatar in initialRF)
        {
            Vector3 position = ij2position(avatar.Item1, avatar.Item2);
            Vector3 temp = transform.position - position;
            Vector3 forward = new Vector3(temp.x, 0, temp.z);
            initialAngles.Add(forward2angle(forward));
        }
        //����Сƫת��ʱ��Ӧ��i
        int n = visitors.Count;
        double minAngle = n * 180;
        int min_i = 0;
        for(int i = 0; i < n; i++)
        {
            double angleSum = 0;
            for(int j = 0; j < n; j++)
            {
                angleSum += deltaAngle(initialAngles[j], sortedVisitors.Keys[(j + i) % n]);
            }
            if(angleSum < minAngle)
            {
                minAngle = angleSum;
                min_i = i;
            }
        }
        //ת��Ϊ���
        for (int i = 0; i < n; i++)
        {
            int visitorIndex = (i + min_i) % n;
            Vector3 position = ij2position(initialRF[i].Item1, initialRF[i].Item2);
            matchedIRF.Add(new Tuple<int, Vector3>(sortedVisitors[sortedVisitors.Keys[visitorIndex]], position));
        }

        //Ĭ��δ����
        /*for (int i = 0; i < initialRF.Count; i++)
        {
            matchedIRF.Add(new Tuple<int, Vector3>(i, ij2position(initialRF[i].Item1, initialRF[i].Item2)));
        }*/

        return matchedIRF;
    }

    //����Ĭ��˳��ƥ���Ԥ�Ƽ�����
    public List<Tuple<int, Vector3>> MatchWithDefaultRF(List<Tuple<Transform, Preference>> visitors)
    {
        List<Tuple<int, int>> initialRF = initialRFs[visitors.Count];
        List<Tuple<int, Vector3>> matchedIRF = new List<Tuple<int, Vector3>>();
        //Ĭ��δ����
        for (int i = 0; i < initialRF.Count; i++)
        {
            matchedIRF.Add(new Tuple<int, Vector3>(i, ij2position(initialRF[i].Item1, initialRF[i].Item2)));
        }
        return matchedIRF;
    }


    int preferences2int(List<Preference> preferences)
    {
        int r = 0;
        int n = Enum.GetValues(typeof(Preference)).Length;
        for(int i = 0; i < preferences.Count; i++)
        {
            r = r * n + ((int)preferences[i]);
        }
        return r;
    }

    List<Vector3> Reorder(List<Tuple<int, Vector3>> rawRF)
    {
        Vector3[] reorderedRF = new Vector3[rawRF.Count];
        foreach(var v in rawRF)
        {
            reorderedRF[v.Item1] = v.Item2;
        }
        return new List<Vector3>(reorderedRF);
    }

    //�ж���Ҫ���¼��㣬�������Ҫ��ֱ�ӷ��ؽ������֮����null
    public List<Vector3> RFContains(List<Tuple<Transform, Preference>> inputAvatars, List<Tuple<int, Vector3>> rawIRF)
    {
        //���û�ж�Ӧ�������Ƽ����Σ�ֱ�ӷ���
        if (!RFs.ContainsKey(inputAvatars.Count))
        {
            return null;
        }
        //rawIRF���ǰ�����Сƫת������õ�����
        List<Preference> preferences = new List<Preference>();
        for(int i = 0; i < rawIRF.Count; i++)
        {
            preferences.Add(inputAvatars[rawIRF[i].Item1].Item2);
        }
        if (RFs[inputAvatars.Count].ContainsKey(preferences2int(preferences)))
        {
            List<Vector3> temp = RFs[inputAvatars.Count][preferences2int(preferences)];
            List<Tuple<int, Vector3>> rawRF = new List<Tuple<int, Vector3>>();
            for(int i = 0; i < temp.Count; i++)
            {
                rawRF.Add(new Tuple<int, Vector3>(rawIRF[i].Item1, temp[i]));
            }
            return Reorder(rawRF);
        }
        return null;
    }

    //ʵʱ�����Ƽ�����
    public List<Vector3> GetRecommendedFormation(List<Tuple<Transform, Preference>> inputAvatars)
    {
        //���δ��ɳ�ʼ����ֱ�ӷ���null
        if (isGettingMaxQualiies) return null;

        //��ȡԤ�Ƽ����Σ��ڴ˻����ϵ���
        if (!initialRFs.ContainsKey(inputAvatars.Count))
        {
            if(!isGettingInitialRF) StartCoroutine("GetInitialRF", inputAvatars.Count);
            return null;
        }

        //������Сƫת��ƥ��Ԥ�Ƽ����Σ�����Ԥ�Ƽ���������int����ӳ���ϵ
        List<Tuple<int, Vector3>> rawIRF = null;
        if (minAngle)
        {
            rawIRF = MatchWithInitialRF(inputAvatars);
        }
        else
        {
            rawIRF = MatchWithDefaultRF(inputAvatars);
        }
        


        //���֮ǰ������ͬ�������¼���
        List<Vector3> rf = RFContains(inputAvatars, rawIRF);
        if (rf != null) return rf;

        Tuple<List<Tuple<int, Vector3>>, List<Tuple<Transform, Preference>>> rawIRFandInputAvatars = new Tuple<List<Tuple<int, Vector3>>, List<Tuple<Transform, Preference>>>(rawIRF, inputAvatars);
        //�����п������ڼ���->����Ԥ�Ƽ����Σ�û�ڼ���->��������Э�̲��ҷ���Ԥ�Ƽ�����
        //if (!isGettingRF) StartCoroutine("GetRF", rawIRFandInputAvatars);

        //���ظ�����Сƫת��ƥ���Ԥ�Ƽ�����
        return Reorder(rawIRF);
    }

    /*--------------------���ݿ��ӻ�--------------------*/
    Vector3[,] positions;
    float[,] temperature;
    float maxTemperature;
    float minTemperature;
    public Color[] TemperatureColors;
    double[,] colorfulnessArray;//sbBug

    IEnumerator InitTemperatures()
    {
        positions = new Vector3[line, line];
        temperature = new float[line, line];
        colorfulnessArray = new double[line, line];//sbBug
        maxTemperature = 0;
        minTemperature = 1;

        List<Point> points = new List<Point>();
        for (int i = 0; i < line; i++)
        {
            for (int j = 0; j < line; j++)
            {
                positions[i, j] = ij2position(i, j);
                points.Add(new Point(i, j, isValid(i, j)));
            }
        }

        int cameraCount = cameraContainers.Count;

        for (int index = 0; index < line * line; index += cameraCount)
        {
            for (int i = 0; i < cameraCount; i++)
            {
                int j = index + i;
                if (j >= line * line) break;
                SetCamera(points[j], i);

            }
            yield return new WaitForEndOfFrame();
            bool sbBug = false; int sbBug_j = 0;//sbBug
            for (int i = 0; i < cameraCount; i++)
            {
                int j = index + i;
                if (j >= line * line) break;
                int l = j / line;
                int c = j % line;
                GameObject cameraContainer = cameraContainers[i];
                if (points[j].takePicture)
                {
                    Quality quality = cameraContainer.GetComponent<VP_Quality>().getQuality();
                    //if (quality.colorfulness_quality > 1) quality.colorfulness_quality = 0.0984432828862205f;
                    UpdateQuality(quality, false);//��������Լ������ܷ���finalQuality
                    ///////////////////////sbBug
/*                    colorfulnessArray[l, c] = quality.colorfulness_quality;
                    if (finalQuality > 1)
                    {
                        finalQuality = temperature[l, c - 1];
                        sbBug = true;
                        sbBug_j = j;
                    }*/
                    ///////////////////////
                    temperature[l, c] = (float)finalQuality;
                    maxTemperature = (float)Math.Max(finalQuality, maxTemperature);
                    minTemperature = (float)Math.Min(finalQuality, minTemperature);
                }
                else
                {
                    temperature[l, c] = 0;//����ǲ��Ϸ����ӵ㣬���Ϊ0
                }
                //UnityEngine.Debug.Log("point[" + j + "], position:" + positions[l, c] + ", score:" + temperature[l, c]);
            }
            if (sbBug) // ��ֵ
            {
                int l = sbBug_j / line;
                int c = sbBug_j % line;
                //UnityEngine.Debug.Log("this color:" + colorfulnessArray[l, c]);
                //UnityEngine.Debug.Log("avg color:" + 0.5 * (colorfulnessArray[l, c - 1] + colorfulnessArray[l, c + 1]));
            }
        }
        //UnityEngine.Debug.Log("max:" + maxTemperature);
        //UnityEngine.Debug.Log("min:" + minTemperature);
    }

    void AddVertexColor()
    {
        MeshFilter meshFilter = transform.parent.GetComponent<MeshFilter>();
        Color[] colors = new Color[meshFilter.mesh.colors.Length];//line * line
        Vector3[] vertices = new Vector3[line * line];
        Vector2[] uv = new Vector2[line * line];
        float deltaHeight = transform.position.y - floorHeight; //չƷ�����ľ����
        for (int j = 0; j < line; j++)
        {
            for (int i = 0; i < line; i++)
            {
                float temperature = this.temperature[j, i];
                if (temperature != 0) temperature = (temperature - minTemperature) / (maxTemperature - minTemperature) * 50 + 50;
                // �����¶�ֵ���㶥����ɫֵ
                colors[line * j + i] = CalcColor(temperature);
                //Vector3 vertex = new Vector3(i * perWidth, j * perHeight, GetHeightByTemperature(temperature));
                Vector3 vertex = new Vector3(positions[j, i].x, GetHeightByTemperature(temperature) + floorHeight, positions[j, i].z);
                vertices[line * j + i] = vertex;
                uv[line * j + i] = new Vector2(0, 1) + new Vector2(1 / line * i, 1 / line * j);
            }
        }
        meshFilter.mesh.colors = colors;
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.uv = uv;
        meshFilter.mesh.RecalculateNormals();
    }

    private Color CalcColor(float temperature)
    {
        if(temperature >= 100) return TemperatureColors[TemperatureColors.Length - 1];
        int count = (int)temperature / 10;
        float temp = (temperature % 10) / 10;
        Color[] colors = GetColors(count);
        Color from = colors[0];
        Color to = colors[1];
        Color offset = to - from;
        return from + offset * temp;
    }

    // TemperatureColors ʵ��Inspector��������õ���ɫ����

    private Color[] GetColors(int index)
    {
        Color startColor = Color.blue, endColor = Color.blue;
        startColor = TemperatureColors[index];
        endColor = TemperatureColors[index + 1];
        return new Color[] { startColor, endColor };
    }

    private float GetHeightByTemperature(float temperature)
    {
        //return (0.5f - (temperature - minTemperature) / (maxTemperature - minTemperature));
        //return temperature / 100;
        if(temperature == 0)
        {
            return -0.1f;
        }
        else
        {
            return 0.1f;
            //return 1.3f * (temperature - 50) / 50;
        }
    }

    void DrawHeatMap()
    {
        GetComponent<GenerateMesh>().Generate(line, line);//�����������������
        AddVertexColor();//���Ķ����������ɫ
    }

    IEnumerator ShowAllScores()
    {
        ChangeLayer(this.transform, "Exhibition");
        yield return StartCoroutine("InitTemperatures");//�����������ڶ�ά����temperature��
        ChangeLayer(this.transform, "Default");
        DrawHeatMap();
    }

    /*--------------------ˢ���߼�--------------------*/

    private void Update()
    {
        /*        if (test)
                {
                    GetRecommendedFormation(testInputAvatars);
                }*/
        if (test != testFlag)
        {
            testFlag = test;
            sw.Restart();
            GetRecommendedFormation(testInputAvatars);
        }
        if (showAllScores)
        {
            showAllScores = false;
            StartCoroutine("ShowAllScores");
        }
    }
}
