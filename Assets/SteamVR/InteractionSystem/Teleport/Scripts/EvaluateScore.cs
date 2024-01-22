using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/*public enum Preference
{
    Close = 1,
    Normal = 2,
    Far = 3,
}*/
public enum Preference
{
    Close = 0,
    Normal = 1,
    Far = 2,
}
public class EvaluateScore : MonoBehaviour
{
    public class PreData
    {
        public double[] sizeQs;
        public double[] intQs;
        public double[] colorQs;
        public double[] depthQs;
        public double maxColorfulnessQuality;
        public double maxDepthQuality;
        public double maxSizeQuality;
        public int line;
    }

    [Serializable]
    public class FormationData
    {
        public OneFormation[] formations;
    }

    [Serializable]
    public class OneFormation
    {
        public int[] preferences;
        public double[] xs;
        public double[] ys;
        public double[] zs;
        public double[] sizeQs;
        public double[] depthQs;
        public double[] colorQs;
        public double[] coverQs;
        public double[] intQs;
        public double[] finalQs;
    }


    public float pre2dis(Preference preference)
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
    class Avatar
    {
        public GameObject avatar;
        //public GameObject arrow;
        public Preference pre = Preference.Normal;
        //public double score;
        public Quality score;
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
        public double quality;
        public bool isValid;
        public void setValue(int i, int j, bool isValid)
        {
            this.i = i;
            this.j = j;
            this.isValid = isValid;
        }
    }

    public GameObject cameraContainer;
    [Tooltip("首先选择人数，点击之后会生成队形")]
    [Header("预处理")]
    public bool isGettingPreQ;

    public bool no_viewpoints;
    private bool nvFlag;
    [Header("实时迭代")]
    public bool measure;
    private bool flag;
    //public int groupScale = 15;
    [Tooltip("分布角度，越小越密")]
    public float angleSum = 360;
    [Tooltip("采样半径")]
    public float far = 4;
    [Tooltip("采样密度")]
    public float gridSpacing = 0.1f;
    [Tooltip("最大迭代次数")]
    public int maxMeasuringTime = 60;
    [Tooltip("人与人之间的最小距离")]
    public float minDis = 0.5f;
    private GameObject avatarModel;
    private GameObject avatarModel_Close;
    private GameObject avatarModel_Far;
    [Header("因子系数")]
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
    [Range(0, 100)]
    public float k_dir = 0;
    [Range(0, 100)]
    public double k_neg = 0;
    [Header("因子质量")]
    public double sizeQuality;
    public double depthQuality;
    public double colorfulnessQuality;
    public double coveredQuality;
    public double integrityQuality;
    public double directionQuality;//暂时没有
    public double negQuality;//不需要
    public double finalQuality;
    public double maxColorfulnessQuality;
    public double maxDepthQuality;
    public double maxSizeQuality;
    [Header("不可更改")]
    public bool isMeasured;
    public bool isMeasuring;
    private bool first;

    //private Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, double>>>> recommendedFormation;
    private Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>>> recommendedFormation;
    private bool[,] validList = null;
    private bool[,] nextList = null;
    private bool[,] nowList = null;
    private PreQuality[,] preQualities = null;
    private int line;
    //private float center_x;
    //private float center_z;
    public float secondFloorHeight = 3.42f;
    private float height;
    private float floorHeight;
    public float scale = 0.05f;
    private GameObject viewpoints;
    private GameObject pointObjsContainer;
    private GameObject arrowObjsContainer;
    private List<Avatar> avatars;
    private Material lineMaterial;
    private SortedList<double, Tuple<int,int>> initialFormation;

    [Header("Test")]
    public int test_i;
    public int test_j;
    public bool test;
    public bool isTesting;
    private bool testFlag;
    public double test_quality;

    void Start()
    {
        if(transform.position.y > secondFloorHeight)
        {
            height = 5.16337f;
            floorHeight = secondFloorHeight;
        }
        else
        {
            height = 2.03003f;
            floorHeight = 0.33f;
        }
        cameraContainer = GameObject.Find("CameraContainer");
        avatarModel = GameObject.Find("FollowerModel");
        avatarModel_Close = GameObject.Find("FollowerModel_Close");
        avatarModel_Far = GameObject.Find("FollowerModel_Far");
        //scalePositionsMap = new Dictionary<int, List<float>>();
        //recommendedFormation = new Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, double>>>>();//（人数，（（偏好序列，位置分数序列）））
        recommendedFormation = new Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>>>();
        isGettingPreQ = measure = flag = isMeasuring  = test = testFlag = isTesting = false;
        first = no_viewpoints = nvFlag = true;
        getValidPositions();

        StartCoroutine("GetPreQuality");
/*        if (no_viewpoints)
        {
            viewpoints.SetActive(false);
        }*/
    }

    void Update()
    {
/*        if (first)
        {
            StartCoroutine("GetPreQuality");
            first = false;
        }*/
        if (!isMeasuring && !isGettingPreQ)
        {
            if (measure != flag)//点击measure按钮
            {
                //StartCoroutine("Measure", groupScale);
                StartCoroutine("MeasureAll");
                flag = measure;//重置measure按钮
            }
        }
        if (test != testFlag && !isGettingPreQ)
        {
            StartCoroutine("testMeasure");
            testFlag = test;
        }
        if(no_viewpoints != nvFlag)
        {
            nvFlag = no_viewpoints;
            viewpoints.SetActive(!no_viewpoints);
        }
    }

    Vector3 ij2position(int i, int j)
    {
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        float x = center_x - far + gridSpacing * i;
        float z = center_z - far + gridSpacing * j;
        return new Vector3(x, height, z);
    }

    List<List<Preference>> genAllCombination(int groupScale)
    {
        List<List<Preference>> preferencesList = new List<List<Preference>>();
        foreach (Preference pre_1 in Enum.GetValues(typeof(Preference)))
        {
            foreach (Preference pre_2 in Enum.GetValues(typeof(Preference)))
            {
                foreach (Preference pre_3 in Enum.GetValues(typeof(Preference)))
                {
                    List<Preference> preferences = new List<Preference>();
                    preferences.Add(pre_1);
                    preferences.Add(pre_2);
                    preferences.Add(pre_3);
                    for (int i = 0; i < groupScale - 3; i++)
                    {
                        preferences.Add(Preference.Normal);
                    }
                    preferencesList.Add(preferences);
                }
            }
        }
        return preferencesList;
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

    void getValidPositions()
    {
        viewpoints = new GameObject("viewpoints");
        viewpoints.transform.SetParent(transform);//视点可视化
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        Vector2 center = new Vector2(center_x, center_z);
        line = Mathf.FloorToInt(2 * far / gridSpacing) + 1;
        validList = new bool[line, line];
        nextList = new bool[line, line];
        nowList = new bool[line, line];
        //validList = new List<List<bool>>();
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
                    Collider[] colliders1 = Physics.OverlapSphere(new Vector3(x, floorHeight-0.1f, z), 0.05f);
/*                    GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    dot.transform.position = new Vector3(x, 0.27f - 0.1f, z);
                    dot.GetComponent<Renderer>().material.color = Color.red;
                    dot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);*/
                    if (colliders.Length == 0 && colliders1.Length != 0)
                    {
                        validList[i, j] = true;
                        //视点可视化
                        GameObject redDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        redDot.transform.SetParent(viewpoints.transform);
                        redDot.transform.position = new Vector3(x, height, z);
                        redDot.GetComponent<Renderer>().material.color = Color.red;
                        redDot.transform.localScale = new Vector3(scale, scale, scale);
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

    double calNegQuality(int index, int ii, int jj)
    {
        Avatar avatar = avatars[index];
        Vector3 position = ij2position(ii, jj);
        double neg = 0;
        double dis2_close = 2 * Math.Pow(gridSpacing, 2);
        double dis2_normal = 4 * Math.Pow(gridSpacing, 2);
        double dis2_far = 5 * Math.Pow(gridSpacing, 2);
        for (int i = 0; i < avatars.Count; i++)
        {
            if (index != i)
            {
                Avatar other = avatars[i];
                Vector3 o_position = ij2position(other.i, other.j);
                double dis2 = Math.Pow((position.x - o_position.x), 2) + Math.Pow((position.z - o_position.z), 2);
                if (dis2 < gridSpacing * gridSpacing / 2)
                {
                    neg += -100;
                }
                else
                {
                    double new_neg = neg - ((int)avatar.pre * (int)other.pre) / dis2;
                    if (other.pre == Preference.Close)
                    {
                        if (dis2 < dis2_close + 0.000001f)
                        {
                            neg = new_neg;
                        }

                    } else if (other.pre == Preference.Normal)
                    {
                        if (dis2 < dis2_normal + 0.000001f)
                        {
                            neg = new_neg;
                        }
                    }
                    else
                    {
                        if (dis2 < dis2_far + 0.000001f)
                        {
                            neg = new_neg;
                        }
                    }
                }
            }
        }
        return neg;
    }

    double calDirQuality()
    {
        return 0;
    }

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
                if(dis < Math.Max(pre2dis(avatars[i].pre), pre2dis(avatars[index].pre)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    List<Preference> GenInitialPreferences(int groupScale) {
        List<Preference> initialPreferences = new List<Preference>();
        for(int i = 0; i < groupScale; i++)
        {
            initialPreferences.Add(Preference.Far);
        }
        return initialPreferences;
    }
    
    List<Avatar> generateAvatars(List<Preference> preferences)
    {
        List<Avatar> avatars = new List<Avatar>();
        int num = preferences.Count;
        //1.计算角度（展示时可以用差点的初始情况），得到坐标
        //2.坐标处生成形象
        float angle = angleSum/num;
        float now_angle = 0;
        for (int avatar_i = 0; avatar_i < num; avatar_i++)
        {
            float r = far;
            now_angle += angle;
            while (true)
            {
                //Debug.Log("isGeneratingAvatars");
                /*r += gridSpacing;
                if (r > far)
                {
                    r = gridSpacing;
                    angle_i += 180;
                }*/
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
                if (isValid(i,j) && !nowList[i, j])
                {
                    bool tooClose = false;
                    //判断是不是太近
                    for(int ii = 0; ii < avatar_i; ii++)
                    {
                        Vector3 position = ij2position(i, j);
                        Vector3 other_position = ij2position(avatars[ii].i, avatars[ii].j);
                        double dis = Math.Sqrt(Math.Pow(position.x - other_position.x, 2) + Math.Pow(position.z - other_position.z, 2));
                        if(dis < Math.Max(pre2dis(avatars[ii].pre), pre2dis(preferences[avatar_i])))
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
                        GameObject avatarObj;
                        if (preferences[avatar_i] == Preference.Close)
                        {
                            avatarObj = GameObject.Instantiate(avatarModel_Close);
                        }
                        else if (preferences[avatar_i] == Preference.Far)
                        {
                            avatarObj = GameObject.Instantiate(avatarModel_Far);
                        }
                        else
                        {
                            avatarObj = GameObject.Instantiate(avatarModel);
                        }
                        avatarObj.transform.position = ij2position(i, j);
                        Vector3 forward = this.transform.position - avatarObj.transform.position;
                        avatarObj.transform.forward = new Vector3(forward.x, 0, forward.z);
                        avatar.avatar = avatarObj;
                        avatar.pre = preferences[avatar_i];
                        avatars.Add(avatar);
                        break;
                    }
                }
            }
        }
        return avatars;
    }

    List<Avatar> generateAvatarsByInitialFormation(List<Preference> preferences)
    {
        //生成形象
        List<Avatar> avatars = new List<Avatar>();
        foreach(var key in initialFormation.Keys)//key是-质量
        {
            int avatar_i = initialFormation.IndexOfKey(key);
            int i = initialFormation[key].Item1;
            int j = initialFormation[key].Item2;
            nowList[i, j] = true;
            Avatar avatar = new Avatar();
            avatar.preI = avatar.i = i;
            avatar.preJ = avatar.j = j;
            GameObject avatarObj;
            if (preferences[avatar_i] == Preference.Close)
            {
                avatarObj = GameObject.Instantiate(avatarModel_Close);
            }
            else if (preferences[avatar_i] == Preference.Far)
            {
                avatarObj = GameObject.Instantiate(avatarModel_Far);
            }
            else
            {
                avatarObj = GameObject.Instantiate(avatarModel);
            }
            avatarObj.transform.position = ij2position(i, j);
            Vector3 forward = this.transform.position - avatarObj.transform.position;
            avatarObj.transform.forward = new Vector3(forward.x, 0, forward.z);
            avatar.avatar = avatarObj;
            avatar.pre = preferences[avatar_i];
            avatars.Add(avatar);
        }
        return avatars;
    }
    
    void ChangeLayer(Transform trans, string targetLayer)
    {
        if (LayerMask.NameToLayer(targetLayer) == -1)
        {
            //Debug.Log("Layer中不存在,请手动添加LayerName");
            return;
        }
        //遍历更改所有子物体layer
        trans.gameObject.layer = LayerMask.NameToLayer(targetLayer);
        foreach (Transform child in trans)
        {
            ChangeLayer(child, targetLayer);
        }
    }

    void DestroyChildren(GameObject father)
    {
        Transform[] children = father.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child == father.transform)
            {
                continue;
            }
            Destroy(child.gameObject);
        }
    }

    void GenLine(GameObject father, Avatar avatar)
    {
        GameObject line = new GameObject();
        line.transform.SetParent(father.transform);
        LineRenderer lineRenderer_i = line.AddComponent<LineRenderer>();
        lineRenderer_i.material = lineMaterial;
        lineRenderer_i.material.color = Color.black;
        lineRenderer_i.startWidth = 0.05f / 4;
        lineRenderer_i.endWidth = 0.05f / 4;
        lineRenderer_i.SetPosition(0, ij2position(avatar.i, avatar.j));
        lineRenderer_i.SetPosition(1, ij2position(avatar.nextI, avatar.nextJ));
        ChangeLayer(line.transform, "JustForShowing");
    }

    bool GotoNextPositions()
    {
        //bool go = false;
        bool oscillating = true;
        foreach (Avatar avatar in avatars)
        {
            nextList[avatar.nextI, avatar.nextJ] = false;
            nowList[avatar.i, avatar.j] = false;
            nowList[avatar.nextI, avatar.nextJ] = true;
            if(!(avatar.preI == avatar.nextI && avatar.preJ == avatar.nextJ))
            {
                oscillating = false;
            }
            avatar.preI = avatar.i;
            avatar.preJ = avatar.j;
            if (!(avatar.i == avatar.nextI && avatar.j == avatar.nextJ))
            {
                //Debug.Log("(" + avatar.i + "," + avatar.j + ")->(" + avatar.nextI + "," + avatar.nextJ + ")");
                //go = true;
                avatar.avatar.transform.position = ij2position(avatar.nextI, avatar.nextJ);
                Vector3 forward = this.transform.position - avatar.avatar.transform.position;
                avatar.avatar.transform.forward = new Vector3(forward.x, 0, forward.z);
                avatar.i = avatar.nextI;
                avatar.j = avatar.nextJ;
                //avatar.arrow.SetActive(false);
            }
        }
        //return go;
        return !oscillating;
    }

    void MoveCamera(int i,int j)
    {
        cameraContainer.transform.rotation = Quaternion.identity;
        cameraContainer.transform.position = ij2position(i, j);
        Vector3 forward = this.transform.position - cameraContainer.transform.position;
        cameraContainer.transform.forward = new Vector3(forward.x, 0, forward.z);
    }

    void UpdateQuality(Quality quality,bool test)
    {
        double k_sum = k_size + k_integrity + k_color + k_depth + k_covered + k_neg + k_dir;
        sizeQuality = quality.size_quality;
        integrityQuality = quality.integrity_quality;
        if (test)
        {
            colorfulnessQuality = quality.colorfulness_quality;
            depthQuality = quality.depth_quality;
            sizeQuality = quality.size_quality;
        }
        else
        {
            colorfulnessQuality = quality.colorfulness_quality / maxColorfulnessQuality;
            depthQuality = quality.depth_quality / maxDepthQuality;
            sizeQuality = quality.size_quality / maxSizeQuality;
        }
        coveredQuality = quality.covered_quality;
        //negQuality = calNegQuality(index, point.i, point.j);
        negQuality = 0;
        directionQuality = calDirQuality();
        finalQuality = (k_size / k_sum) * sizeQuality + (k_integrity / k_sum) * integrityQuality +
            (k_color / k_sum) * colorfulnessQuality + (k_depth / k_sum) * depthQuality +
            (k_covered / k_sum) * coveredQuality + (k_neg / k_sum) * negQuality + (k_dir / k_sum) * directionQuality;
    }
    
    void UpdateQuality(PreQuality preQuality)
    {
        sizeQuality = preQuality.size_quality;
        integrityQuality = preQuality.integrity_quality;
        colorfulnessQuality = preQuality.colorfulness_quality;
        depthQuality = preQuality.depth_quality;
    }
    
    void ReadPreData(PreData preData)
    {
        maxColorfulnessQuality = preData.maxColorfulnessQuality;
        maxDepthQuality = preData.maxDepthQuality;
        maxSizeQuality = preData.maxSizeQuality;
        preQualities = new PreQuality[preData.line, preData.line];
        for(int i = 0; i < preData.intQs.Length; i++)
        {
            double s_q = preData.sizeQs[i];
            double i_q = preData.intQs[i];
            double c_q = preData.colorQs[i];
            double d_q = preData.depthQs[i];
            PreQuality preQuality = new PreQuality(s_q, d_q, c_q, i_q);
            preQualities[i / line, i % line] = preQuality;
        }
    }
    
    IEnumerator GetPreQuality()
    {
        isGettingPreQ = true;
        viewpoints.SetActive(false);
        ChangeLayer(this.transform, "Exhibition");

        string directory = Application.dataPath + "/Resources/PreQuality";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        string path = directory + "/" + this.gameObject.name + ".json";
        //1.试图从文件读取，将文件内容读入到preQualities以及max中
        bool canRead = false;
        if (File.Exists(path))
        {
            string jsonFromFile = File.ReadAllText(path);
            PreData preData = JsonUtility.FromJson<PreData>(jsonFromFile);
            if(preData.line == line)
            {
                canRead = true;
                ReadPreData(preData);
            }
        }
        //2.如果没有则遍历测量并且写回，无效部分填0
        if (!canRead)
        {
            PreData preData = new PreData();
            preData.intQs = new double[line * line];
            preData.sizeQs = new double[line * line];
            preData.colorQs = new double[line * line];
            preData.depthQs = new double[line * line];
            preQualities = new PreQuality[line, line];
            maxColorfulnessQuality = maxDepthQuality = maxSizeQuality = 0;
            for (int i = 0; i < line; i++)
            {
                for (int j = 0; j < line; j++)
                {
                    PreQuality preQuality;
                    if (isValid(i, j))
                    {
                        MoveCamera(i, j);
                        preQuality = cameraContainer.GetComponent<VP_Quality>().getPreQuality();
                        yield return 0;
                        preQuality = cameraContainer.GetComponent<VP_Quality>().getPreQuality();
                        UpdateQuality(preQuality);//更新面板
                        if (colorfulnessQuality > maxColorfulnessQuality)
                        {
                            maxColorfulnessQuality = colorfulnessQuality;
                        }
                        if (depthQuality > maxDepthQuality)
                        {
                            maxDepthQuality = depthQuality;
                        }
                        if (sizeQuality > maxSizeQuality)
                        {
                            maxSizeQuality = sizeQuality;
                        }

                    }
                    else
                    {
                        preQuality = new PreQuality(0, 0, 0, 0);
                    }
                    preData.sizeQs[i * line + j] = preQuality.size_quality;
                    preData.intQs[i * line + j] = preQuality.integrity_quality;
                    preData.colorQs[i * line + j] = preQuality.colorfulness_quality;
                    preData.depthQs[i * line + j] = preQuality.depth_quality;
                    preQualities[i, j] = preQuality;
                }
            }
            preData.maxColorfulnessQuality = maxColorfulnessQuality;
            preData.maxDepthQuality = maxDepthQuality;
            preData.maxSizeQuality = maxSizeQuality;
            preData.line = line;
            string json = JsonUtility.ToJson(preData);
            File.WriteAllText(path, json);
        }

        viewpoints.SetActive(!no_viewpoints);
        ChangeLayer(this.transform, "Default");
        isGettingPreQ = false;
    }

    //供外部调用！！！
/*    public Tuple<Quality,double> CalFinalQuality(PreQuality preQuality, double c_q)
    {
        double k_sum = k_size + k_integrity + k_color + k_depth + k_covered + k_neg + k_dir;
        integrityQuality = preQuality.integrity_quality;
        colorfulnessQuality = preQuality.colorfulness_quality / maxColorfulnessQuality;
        depthQuality = preQuality.depth_quality / maxDepthQuality;
        sizeQuality = preQuality.size_quality / maxSizeQuality;
        coveredQuality = c_q;
        negQuality = directionQuality = 0;
        finalQuality = (k_size / k_sum) * sizeQuality + (k_integrity / k_sum) * integrityQuality +
            (k_color / k_sum) * colorfulnessQuality + (k_depth / k_sum) * depthQuality +
            (k_covered / k_sum) * coveredQuality + (k_neg / k_sum) * negQuality + (k_dir / k_sum) * directionQuality;
        Quality quality = new Quality(sizeQuality, depthQuality, colorfulnessQuality, coveredQuality, integrityQuality);
        return new Tuple<Quality, double>(quality, finalQuality);
    }*/

    public Tuple<Quality, double> CalFinalQuality(Quality quality)
    {
        double k_sum = k_size + k_integrity + k_color + k_depth + k_covered + k_neg + k_dir;
        integrityQuality = quality.integrity_quality;
        colorfulnessQuality = quality.colorfulness_quality / maxColorfulnessQuality;
        depthQuality = quality.depth_quality / maxDepthQuality;
        sizeQuality = quality.size_quality / maxSizeQuality;
        coveredQuality = quality.covered_quality;
        negQuality = directionQuality = 0;
        finalQuality = (k_size / k_sum) * sizeQuality + (k_integrity / k_sum) * integrityQuality +
            (k_color / k_sum) * colorfulnessQuality + (k_depth / k_sum) * depthQuality +
            (k_covered / k_sum) * coveredQuality + (k_neg / k_sum) * negQuality + (k_dir / k_sum) * directionQuality;
        //Quality quality = new Quality(sizeQuality, depthQuality, colorfulnessQuality, coveredQuality, integrityQuality);
        return new Tuple<Quality, double>(quality, finalQuality);
    }

    IEnumerator testMeasure()
    {
        isTesting = true;
        viewpoints.SetActive(false);
        ChangeLayer(this.transform, "Exhibition");
        cameraContainer.transform.rotation = Quaternion.identity;
        //cameraContainer.transform.position = ij2position(test_i, test_j);
        Vector3 forward = this.transform.position - cameraContainer.transform.position;
        cameraContainer.transform.forward = new Vector3(forward.x, 0, forward.z);
        Quality quality = cameraContainer.GetComponent<VP_Quality>().getQuality();
        yield return 0; 
        quality = cameraContainer.GetComponent<VP_Quality>().getQuality();
        UpdateQuality(quality, true);
        isTesting = false;
    }
    
    IEnumerator GetBalanced()
    {//avatars中是初始形象
        for (int measuringTime = 1; measuringTime <= maxMeasuringTime; measuringTime++) 
        {
            //Debug.Log("---------------------[measureTime=" + measuringTime + "]---------------------");
            for (int index = 0; index < avatars.Count; index++)
            {
                Avatar avatar = avatars[index];
                Vector3 position = avatar.avatar.transform.position;
                ChangeLayer(avatar.avatar.transform, "JustForShowing");
                avatar.avatar.transform.Find("Flag").gameObject.SetActive(true);
                List<Point> points = generatePoints(avatar.i, avatar.j);
                double maxQuality = -100;
                for (int i = 0; i < points.Count; i++)
                {
                    Point point = points[i];
                    if (isValid(point.i, point.j) && !nextList[point.i, point.j] && (!nowList[point.i, point.j] || i == 0) && (!TooClose(index, point.i, point.j) || i == 0))//有效的并且没人并且没人去
                    {
                        MoveCamera(point.i, point.j);
                        double c_q = cameraContainer.GetComponent<VP_Quality>().getCoveredQuality();
                        yield return 0;
                        c_q = cameraContainer.GetComponent<VP_Quality>().getCoveredQuality();
                        Quality quality = new Quality(preQualities[point.i, point.j].size_quality, preQualities[point.i, point.j].depth_quality, preQualities[point.i, point.j].colorfulness_quality, c_q, preQualities[point.i, point.j].integrity_quality);
                        UpdateQuality(quality, false);//更新面板以及最终总分数finalQuality
                        if (i == 0)
                        {
                            maxQuality = finalQuality;
                            //Debug.Log("[avatar" + index + "] initial state:([" + point.i + "," + point.j + ")," + finalQuality + "]");
                            //avatar.score = finalQuality;
                            avatar.nextI = point.i;
                            avatar.nextJ = point.j;
                        }
                        else
                        {
                            if (maxQuality < finalQuality)
                            {
                                //Debug.Log("[avatar" + index + "] updateMax:[(" + avatar.nextI + "," + avatar.nextJ + ")," + maxQuality + "] -> [(" + point.i + "," + point.j + "),quality:" + finalQuality + ",neg:" + negQuality + "]");
                                maxQuality = finalQuality;
                                avatar.nextI = point.i;
                                avatar.nextJ = point.j;
                            }
                        }
                        //显示颜色
/*                        Color color;
                        if (finalQuality < -50)
                        {
                            color = Color.gray;
                        }
                        else
                        {
                            float t = Mathf.InverseLerp(0, 1, (float)finalQuality);
                            color = Color.Lerp(Color.blue, Color.red, t);
                        }
                        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        dot.transform.SetParent(pointObjsContainer.transform);
                        dot.transform.position = ij2position(point.i, point.j);
                        dot.transform.position = new Vector3(dot.transform.position.x, 2, dot.transform.position.z);
                        dot.GetComponent<Renderer>().material.color = color;
                        dot.transform.localScale = new Vector3(scale, scale, scale);
                        ChangeLayer(dot.transform, "JustForShowing");*/
                    }
                }
                nextList[avatar.nextI, avatar.nextJ] = true;//将下一处标记，其他人不能去
                                                            //yield return new WaitForSeconds(1);
                ChangeLayer(avatar.avatar.transform, "Default");
                avatar.avatar.transform.Find("Flag").gameObject.SetActive(false);
                //GenLine(arrowObjsContainer, avatar);//显示箭头     
                //DestroyChildren(pointObjsContainer);//删除圆点
            }
            bool go = GotoNextPositions();
            //DestroyChildren(arrowObjsContainer);//删除箭头
            //Debug.Log("go=" + go);
            if (!go)
            {
                break;
            }
        }
    }
    
    IEnumerator GetInitialFormation()
    {
        initialFormation = new SortedList<double, Tuple<int, int>>();
        foreach (Avatar avatar in avatars)
        {
            ChangeLayer(avatar.avatar.transform, "JustForShowing");
            MoveCamera(avatar.i, avatar.j);
            double c_q = cameraContainer.GetComponent<VP_Quality>().getCoveredQuality();
            yield return 0;
            c_q = cameraContainer.GetComponent<VP_Quality>().getCoveredQuality();
            Quality quality = new Quality(preQualities[avatar.i, avatar.j].size_quality, preQualities[avatar.i, avatar.j].depth_quality, preQualities[avatar.i, avatar.j].colorfulness_quality, c_q, preQualities[avatar.i, avatar.j].integrity_quality);
            UpdateQuality(quality, false);//更新面板以及最终总分数finalQuality
            initialFormation.Add(-finalQuality, new Tuple<int, int>(avatar.i, avatar.j));//质量取复数，实现从大到小排序
            ChangeLayer(avatar.avatar.transform, "Default");
        }
    }
    
    IEnumerator SaveScores(List<Preference> preferences)
    {
        double scoreSum = 0;
        List<Tuple<double, double, double, Quality, double>> positionsScoreList = new List<Tuple<double, double, double, Quality, double>>();
        foreach (Avatar avatar in avatars)
        {
            ChangeLayer(avatar.avatar.transform, "JustForShowing");
            MoveCamera(avatar.i, avatar.j);
            double c_q = cameraContainer.GetComponent<VP_Quality>().getCoveredQuality();
            yield return 0;
            c_q = cameraContainer.GetComponent<VP_Quality>().getCoveredQuality();
            Quality quality = new Quality(preQualities[avatar.i, avatar.j].size_quality, preQualities[avatar.i, avatar.j].depth_quality, preQualities[avatar.i, avatar.j].colorfulness_quality, c_q, preQualities[avatar.i, avatar.j].integrity_quality);
            UpdateQuality(quality, false);//更新面板以及最终总分数finalQuality
            //avatar.score = finalQuality;
            avatar.score = new Quality(sizeQuality, depthQuality, colorfulnessQuality, coveredQuality, integrityQuality);
            Vector3 position = ij2position(avatar.i, avatar.j);
            positionsScoreList.Add(new Tuple<double, double, double, Quality, double>(position.x, position.y, position.z, avatar.score, finalQuality));
            scoreSum += finalQuality;
            ChangeLayer(avatar.avatar.transform, "Default");
            /*string directory = Application.dataPath + "/Resources/PreQuality";
            cameraContainer.GetComponent<VP_Quality>().saveImg()*/
        }
        recommendedFormation[avatars.Count][preferences] = positionsScoreList;
        //Debug.Log("一次迭代完成，最终整体分数：" + scoreSum);
        //yield return new WaitForSeconds(30);
    }

    void Restore()
    {
        foreach (Avatar a in avatars)//重置avatars，销毁游戏对象
        {
            Destroy(a.avatar);
        }
        avatars.Clear();
        //将nowList、nextList还原
        for (int i = 0; i < line; i++)
        {
            for (int j = 0; j < line; j++)
            {
                nowList[i, j] = nextList[i, j] = false;
            }
        }
    }

    IEnumerator SaveResult()
    {
        string directory = Application.dataPath + "/Resources/RecommendedFormation";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        //private Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, Quality,double>>>> recommendedFormation
        foreach (var key in recommendedFormation.Keys)
        {
            string path = directory + "/" + this.gameObject.name + "_" + key +".json";
            if (File.Exists(path))
            {
                //Debug.Log("该队形已存在：" + path + ",如要重计算，请手动删除原文件");
            }
            else
            {
                Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>> formationOfN = recommendedFormation[key];
                FormationData formationData = new FormationData();
                formationData.formations = new OneFormation[formationOfN.Count];
                int i = 0;
                foreach (KeyValuePair<List<Preference>, List<Tuple<double, double, double, Quality, double>>> kv in formationOfN)
                {
                    OneFormation oneFormation = new OneFormation();
                    oneFormation.preferences = new int[key];
                    oneFormation.xs = new double[key];
                    oneFormation.ys = new double[key];
                    oneFormation.zs = new double[key];
                    oneFormation.sizeQs = new double[key];
                    oneFormation.depthQs = new double[key];
                    oneFormation.colorQs = new double[key];
                    oneFormation.coverQs = new double[key];
                    oneFormation.intQs = new double[key];
                    oneFormation.finalQs = new double[key];
                    for(int j = 0; j < key; j++)
                    {
                        oneFormation.preferences[j] = (int)kv.Key[j];
                        oneFormation.xs[j] = kv.Value[j].Item1;
                        oneFormation.ys[j] = kv.Value[j].Item2;
                        oneFormation.zs[j] = kv.Value[j].Item3;
                        oneFormation.sizeQs[j] = kv.Value[j].Item4.size_quality;
                        oneFormation.depthQs[j] = kv.Value[j].Item4.depth_quality;
                        oneFormation.colorQs[j] = kv.Value[j].Item4.colorfulness_quality;
                        oneFormation.coverQs[j] = kv.Value[j].Item4.covered_quality;
                        oneFormation.intQs[j] = kv.Value[j].Item4.integrity_quality;
                        oneFormation.finalQs[j] = kv.Value[j].Item5;
                    }
                    formationData.formations[i] = oneFormation;
                    i++;
                }
                string json = JsonUtility.ToJson(formationData);
                File.WriteAllText(path, json);
            }
        }
        yield return 0;
    }

    /*    IEnumerator Measure(int groupScale)
        {
            isMeasuring = true;
            viewpoints.SetActive(false);
            ChangeLayer(this.transform, "Exhibition");

            //检查
            if (groupScale <= 0)
            {
                isMeasuring = false;
                yield break;
                //return;
            }
            if (recommendedFormation.ContainsKey(groupScale))
            {
                recommendedFormation[groupScale].Clear();
            }
            else
            {
                recommendedFormation.Add(groupScale, new Dictionary<List<Preference>, List<Tuple<double, double, double, double>>>());
            }
            //0.前3个人不同pre组合
            //1.生成groupScale个初始点（圆形分布），每个点生成虚拟形象
            //2.循环，每次：对每个角色进行视点质量度量，并且在周围（判定有效性）采样计算迭代方向（可以显示一个箭头）
            //3.最终保存坐标
            List<List<Preference>> preferencesList = genAllCombination(groupScale);
            pointObjsContainer = new GameObject("pointObjsContainer");//用于展示周围视点高低
            arrowObjsContainer = new GameObject("arrowObjsContainer");//用于展示去向
            foreach (List<Preference> preferences in preferencesList)
            {
                avatars = generateAvatars(preferences);//1.生成虚拟形象
                yield return StartCoroutine("GetBalanced");//2.计算平衡位置
                //3.将avatars位置、图像、分数保存，这里需要最后再测量一次每个人的分数
                yield return StartCoroutine("SaveScores");
                List<Tuple<double, double, double, double>> positionsScoreList = new List<Tuple<double, double, double, double>>();
                for (int i = 0; i < avatars.Count; i++)
                {
                    Avatar avatar = avatars[i];
                    Vector3 position = ij2position(avatar.i, avatar.j);
                    positionsScoreList.Add(new Tuple<double, double, double,double>(position.x, position.y, position.z, avatar.score));
                }
                recommendedFormation[groupScale][preferences] = positionsScoreList;
                foreach (Avatar a in avatars)//重置avatars，销毁游戏对象
                {
                    Destroy(a.avatar);
                }
                avatars.Clear();
            }
            isMeasuring = false;
            ChangeLayer(this.transform, "Default");
            //将最终结果输出到文件里
            SaveResult();
        }
    */

    IEnumerator Measure(int groupScale)
    {
        isMeasuring = true;
        viewpoints.SetActive(false);
        ChangeLayer(this.transform, "Exhibition");

        //检查
        if (groupScale <= 0)
        {
            isMeasuring = false;
            yield break;
            //return;
        }
        if (recommendedFormation.ContainsKey(groupScale))
        {
            recommendedFormation[groupScale].Clear();
        }
        else
        {
            //recommendedFormation.Add(groupScale, new Dictionary<List<Preference>, List<Tuple<double, double, double, double>>>());
            recommendedFormation.Add(groupScale, new Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>>());
        }

        pointObjsContainer = new GameObject("pointObjsContainer");//用于展示周围视点高低
        arrowObjsContainer = new GameObject("arrowObjsContainer");//用于展示去向
        //先用间隔最大的情况计算推荐队形，并且给出分数最高的前三个位置，然后每次替换
        List<Preference> initialPreferences = GenInitialPreferences(groupScale);
        avatars = generateAvatars(initialPreferences);
        yield return StartCoroutine("GetBalanced");
        yield return StartCoroutine("GetInitialFormation");//获得后续迭代的初始队形，按分数顺序排序
        Restore();

        //0.前3个人不同pre组合
        //1.生成groupScale个初始点（圆形分布），每个点生成虚拟形象
        //2.循环，每次：对每个角色进行视点质量度量，并且在周围（判定有效性）采样计算迭代方向（可以显示一个箭头）
        //3.最终保存坐标
        List<List<Preference>> preferencesList = genAllCombination(groupScale);
        foreach (List<Preference> preferences in preferencesList)
        {
            avatars = generateAvatarsByInitialFormation(preferences);//1.生成虚拟形象
            yield return StartCoroutine("GetBalanced");//2.计算平衡位置
            yield return StartCoroutine("SaveScores",preferences);//3.将avatars位置、图像、分数保存，这里需要最后再测量一次每个人的分数
            Restore();//初始化
        }
        isMeasuring = false;
        ChangeLayer(this.transform, "Default");
    }

    IEnumerator MeasureAll()
    {
        yield return StartCoroutine("Measure", 15);
        yield return StartCoroutine("Measure", 10);
        yield return StartCoroutine("Measure", 5);
        //这个地方和Identification的ReadRecommendedFormation()需要匹配
        yield return StartCoroutine("SaveResult");
    }
}
