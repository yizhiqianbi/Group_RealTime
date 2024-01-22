/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBalanced : MonoBehaviour
{
    enum Preference
    {
        Close = 1,
        Normal = 2,
        Far = 3
    }
    class Avatar {
        public GameObject avatar;
        public GameObject arrow;
        public Preference pre = Preference.Normal;
        public int i;
        public int j;
        public int nextI;
        public int nextJ;
    }
    struct Point
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
    public bool measure;
    private bool flag;
    public int groupScale;
    [Tooltip("采样半径")]
    public float far = 4;
    [Tooltip("采样密度")]
    public float gridSpacing = 0.5f;
    public GameObject avatarModel;
    [Range(0, 100)]
    public double k_neg;
    [Header("不可更改")]
    public bool isMeasured;
    public bool isMeasuring;
    public Dictionary<int, List<float>> scalePositionsMap;

    private bool[,] validList = null;
    private double[,] negQuality;
    //private List<List<bool>> validList;
    private float height = 1.7f;
    public float scale = 0.05f;
    private GameObject viewpoints;
    private List<Avatar> avatars;

    [Header("Test")]
    public int test_i;
    public int test_j;
    public bool test;
    public bool isTesting;
    private bool testFlag;
    public double test_quality;
    // Start is called before the first frame update
    void Start()
    {
        cameraContainer = GameObject.Find("CameraContainer");
        scalePositionsMap = new Dictionary<int, List<float>>();
        measure = flag = isMeasuring = testFlag = test = false;
        //measure = true;
        getValidPositions();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isMeasuring)
        {
            if (measure != flag)//点击measure按钮
            {
                //isMeasuring = true;
                //Measure(groupScale);
                StartCoroutine("Measure", groupScale);
                //isMeasuring = false;
                flag = measure;//重置measure按钮
            }
        }
        if(test != testFlag)
        {
            StartCoroutine("testMeasure");
            testFlag = test;
        }
    }

    IEnumerator testMeasure()
    {
        isTesting = true;
        cameraContainer.transform.rotation = Quaternion.identity;
        cameraContainer.transform.position = ij2position(test_i, test_j);
        Vector3 forward = this.transform.position - cameraContainer.transform.position;
        cameraContainer.transform.forward = new Vector3(forward.x, 0, forward.z);
        for(int i = 0; i < 2; i++)
        {
            test_quality = cameraContainer.GetComponent<VP_Quality>().getQuality(test_i, test_j);
            int num = i * 2 + 1;
            Debug.Log(num + ".quality:" + test_quality);
            test_quality = cameraContainer.GetComponent<VP_Quality>().getQuality(test_i, test_j);
            num = i * 2 + 2;
            Debug.Log(num + ".quality:" + test_quality);
            yield return 0;
        }
        isTesting = false;
    }

    Vector3 ij2position(int i, int j)
    {
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        float x = center_x - far + gridSpacing * i;
        float z = center_z - far + gridSpacing * j;
        return new Vector3(x, height, z);
    }

    Point genPoint(int i, int j)
    {
        Point point = new Point();
        int len = validList.GetLength(0);
        if (i >= len || j >=len || i<0 || j < 0)
        {
            point.setValue(i, j, false);
        }
        else
        {
            point.setValue(i, j, validList[i, j]);
        }
        return point;
    }
    List<Point> generatePoints(int i, int j)
    {
        List<Point> points = new List<Point>();
        points.Add(genPoint(i - 1, j - 1));
        points.Add(genPoint(i    , j - 1));
        points.Add(genPoint(i + 1, j - 1));
        points.Add(genPoint(i - 1, j    ));
        points.Add(genPoint(i    , j    ));
        points.Add(genPoint(i + 1, j    ));
        points.Add(genPoint(i - 1, j + 1));
        points.Add(genPoint(i    , j + 1));
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
        int line = Mathf.FloorToInt(2 * far / gridSpacing) + 1;
        validList = new bool[line, line];
        negQuality = new double[line, line];
        //validList = new List<List<bool>>();
        for (int i = 0;i<line;i++)
        {
            for (int j=0;j<line;j++)
            {
                float x = center_x - far + gridSpacing * i;
                float z = center_z - far + gridSpacing * j;
                Vector2 point = new Vector2(x, z);
                float distance = Vector2.Distance(center, point);
                if (distance <= far)
                {
                    Collider[] colliders = Physics.OverlapSphere(new Vector3(x, 1.7f, z), 0.05f);
                    if (colliders.Length == 0)
                    {
                        validList[i,j] = true;
                        //视点可视化
                        GameObject redDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        redDot.transform.SetParent(viewpoints.transform);
                        redDot.transform.position = new Vector3(x, height, z);
                        redDot.GetComponent<Renderer>().material.color = Color.red;
                        redDot.transform.localScale = new Vector3(scale, scale, scale);
                    }
                    else
                    {
                        validList[i,j] = false;
                    }
                }
                else
                {
                    validList[i,j] = false;
                }
            }
        }
    }

    void addANeg(int i,int j,double neg)
    {
        int len = negQuality.GetLength(0);
        if(i>=0 && i<len && j>=0 && j < len)
        {
            negQuality[i, j] += neg;
        }
    }
    void addNegQuality(int i,int j)
    {

        addANeg(i-1, j-1, -10);
        addANeg(i-1, j  , -10);
        addANeg(i-1, j+1, -10);
        addANeg(i,   j-1, -10);
        addANeg(i,   j  , -20);
        addANeg(i,   j+1, -10);
        addANeg(i+1, j-1, -10);
        addANeg(i+1, j  , -10);
        addANeg(i+1, j+1, -10);
    }
    void updateNegQuality()
    {
        int len = negQuality.GetLength(0);
        for(int i = 0; i < len; i++)
        {
            for(int j = 0; j < len; j++)
            {
                negQuality[i, j] = 0;
            }
        }
        foreach(Avatar avatar in avatars)
        {
            addNegQuality(avatar.i, avatar.j);
        }
    }

    double calNegQuality(int avatar_i,int avatar_j,int i,int j)
    {
        if(avatar_i==i && avatar_j == j)
        {
            return negQuality[i, j] + 20;
        }
        else
        {
            return negQuality[i, j] + 10;
        }
    }

    //实验版本
    *//*List<Avatar> generateAvatars(int num)
    {
        //1.计算角度（展示时可以用差点的初始情况），得到坐标
        //2.坐标处生成形象
        List<Avatar> avatars = new List<Avatar>(num);
        //Avatar[] avatars = new Avatar[num];
        float angle = 360 / num;
        float r = 0;
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        for (int avatar_i = 0; avatar_i < num; avatar_i++)
        {
            while (true)
            {
                Debug.Log("isGeneratingAvatars");
                r += 0.5f;
                float x = r * Mathf.Cos(Mathf.Deg2Rad * avatar_i * angle);
                float z = r * Mathf.Sin(Mathf.Deg2Rad * avatar_i * angle);
                int i = Mathf.FloorToInt((x + far) / gridSpacing);
                int j = Mathf.FloorToInt((z + far) / gridSpacing);
                if (validList[i, j])
                {
                    Avatar avatar = new Avatar();
                    avatar.i = i;
                    avatar.j = j;
                    GameObject avatarObj = GameObject.Instantiate(avatarModel);
                    avatarObj.transform.position = ij2position(i,j);
                    Vector3 forward = this.transform.position - avatarObj.transform.position;
                    avatarObj.transform.forward = new Vector3(forward.x, 0, forward.z);
                    avatar.avatar = avatarObj;
                    avatars.Add(avatar);
                    break;
                }
            }

        }
        return avatars;
    }*//*

    //展示版本
    List<Avatar> generateAvatars(int num)
    {
        //1.计算角度（展示时可以用差点的初始情况），得到坐标
        //2.坐标处生成形象
        List<Avatar> avatars = new List<Avatar>(num);
        //Avatar[] avatars = new Avatar[num];
        float angle = 360 / num;
        float r = far;
        float center_x = this.gameObject.transform.position.x;
        float center_z = this.gameObject.transform.position.z;
        for (int avatar_i = 0; avatar_i < num; avatar_i++)
        {
            while (true)
            {
                Debug.Log("isGeneratingAvatars");
                float x = r * Mathf.Cos(Mathf.Deg2Rad * avatar_i * angle/3);
                float z = r * Mathf.Sin(Mathf.Deg2Rad * avatar_i * angle/3);
                int i = Mathf.FloorToInt((x + far) / gridSpacing);
                int j = Mathf.FloorToInt((z + far) / gridSpacing);
                if (validList[i, j])
                {
                    Avatar avatar = new Avatar();
                    avatar.i = i;
                    avatar.j = j;
                    GameObject avatarObj = GameObject.Instantiate(avatarModel);
                    avatarObj.transform.position = ij2position(i, j);
                    Vector3 forward = this.transform.position - avatarObj.transform.position;
                    avatarObj.transform.forward = new Vector3(forward.x, 0, forward.z);
                    avatar.avatar = avatarObj;
                    avatars.Add(avatar);
                    break;
                }
                r -= 0.5f;
            }

        }

        return avatars;
    }


    *//*    void CalculateDirections(List<Avatar> avatars)
        {
            //1.调整相机，计算视点质量（将图片保存到本地看看），将自己暂时隐藏
            //2.选择视点质量高的方向，设置nextI，nextJ
            //3.显示箭头
            foreach(Avatar avatar in avatars)
            {
                Vector3 position = avatar.avatar.transform.position;
                avatar.avatar.SetActive(false);
                viewpoints.SetActive(false);
                List<Point> points = generatePoints(avatar.i, avatar.j);
                double maxQuality = -1;
                for(int i = 0; i < points.Count; i++)
                {
                    Point point = points[i];
                    if (point.isValid)
                    {
                        cameraContainer.transform.rotation = Quaternion.identity;
                        cameraContainer.transform.position = ij2position(point.i, point.j);
                        Vector3 forward = this.transform.position - cameraContainer.transform.position;
                        cameraContainer.transform.forward = new Vector3(forward.x, 0, forward.z);
                        double quality = cameraContainer.GetComponent<VP_Quality>().getQuality(point.i, point.j);
                        if (maxQuality < quality)
                        {
                            //Debug.Log("maxQuality=" + maxQuality + ", quality=" + quality + ", position=" + point.i + "," + point.j);
                            Debug.Log("updateMax:[(" + avatar.nextI + "," + avatar.nextJ + ")," + maxQuality + "] -> [(" + point.i + "," + point.j + ")," + quality + "]");
                            maxQuality = quality;
                            avatar.nextI = point.i;
                            avatar.nextJ = point.j;
                        }
                        //yield return 0;
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                avatar.avatar.SetActive(true);
                viewpoints.SetActive(true);
                //显示箭头

            }
        }*//*

    bool GotoNextPositions()
    {
        bool go = false;
        foreach(Avatar avatar in avatars)
        {
            if(!(avatar.i == avatar.nextI && avatar.j == avatar.nextJ))
            {
                //Debug.Log("avatar.i=" + avatar.i + ", avatar.nextI=" + avatar.nextI + ", avatar.j=" + avatar.j + ", avatar.nextJ=" + avatar.nextJ);
                Debug.Log("(" + avatar.i + "," + avatar.j + ")->(" + avatar.nextI + "," + avatar.nextJ + ")");
                go = true;
                avatar.avatar.transform.position = ij2position(avatar.nextI, avatar.nextJ);
                Vector3 forward = this.transform.position - avatar.avatar.transform.position;
                avatar.avatar.transform.forward = new Vector3(forward.x, 0, forward.z);
                avatar.i = avatar.nextI;
                avatar.j = avatar.nextJ;
                //avatar.arrow.SetActive(false);
            }
        }
        return go;
    }

    void SavePositions()
    {
        
    }
    IEnumerator Measure(int groupScale)
    {
        isMeasuring = true;
        if (groupScale == 0)
        {
            isMeasuring = false;
            yield break;
            //return;
        }
        if (scalePositionsMap.ContainsKey(groupScale))
        {
            scalePositionsMap[groupScale].Clear();
        }
        else
        {
            scalePositionsMap.Add(groupScale, new List<float>());
        }
        //0.前3个人不同pre组合
        //1.生成groupScale个初始点（圆形分布），每个点生成虚拟形象
        //2.循环，每次：对每个角色进行视点质量度量，并且在周围（判定有效性）采样计算迭代方向（可以显示一个箭头）
        //3.最终保存坐标
        //List<Avatar> avatars = generateAvatars(groupScale);



        avatars = generateAvatars(groupScale);
        updateNegQuality();
        int measuringTime = 20;

        while (measuringTime > 0)//防止无限迭代
        {
            Debug.Log("measureTime=" + measuringTime);
            //////////////////////////////////////////////////////////////
            ///CalculateDirections(avatars);
            for(int index = 0; index < avatars.Count; index++)
            {
                Avatar avatar = avatars[index];
*//*            foreach (Avatar avatar in avatars)
            {*//*
                Vector3 position = avatar.avatar.transform.position;
                //avatar.avatar.SetActive(false);
                viewpoints.SetActive(false);
                List<Point> points = generatePoints(avatar.i, avatar.j);
                double maxQuality = -100;
                for (int i = 0; i < points.Count; i++)
                {
                    Point point = points[i];
                    if (point.isValid)
                    {
                        cameraContainer.transform.rotation = Quaternion.identity;
                        cameraContainer.transform.position = ij2position(point.i, point.j);
                        Vector3 forward = this.transform.position - cameraContainer.transform.position;
                        cameraContainer.transform.forward = new Vector3(forward.x, 0, forward.z);
                        double quality;
                        quality = cameraContainer.GetComponent<VP_Quality>().getQuality(point.i, point.j);
                        for (int sb = 0; sb < 2; sb++)
                        {
                            quality = cameraContainer.GetComponent<VP_Quality>().getQuality(point.i, point.j);
                            quality = cameraContainer.GetComponent<VP_Quality>().getQuality(point.i, point.j);
                            yield return 0;
                        }
                        double negQuality = calNegQuality(avatar.i, avatar.j, point.i, point.j);
                        quality = 50 / (50 + k_neg) * quality + k_neg / (50 + k_neg) * 0.1*negQuality;
                        if (maxQuality < quality)
                        {
                            //Debug.Log("maxQuality=" + maxQuality + ", quality=" + quality + ", position=" + point.i + "," + point.j);
                            Debug.Log("updateMax:[(" + avatar.nextI + "," + avatar.nextJ + ")," + maxQuality + "] -> [(" + point.i + "," + point.j + ")," + quality + "]");
                            maxQuality = quality;
                            avatar.nextI = point.i;
                            avatar.nextJ = point.j;
                        }
                    }
                }
                avatar.avatar.SetActive(true);
                viewpoints.SetActive(true);
                //显示箭头

            }
            bool go = GotoNextPositions();
            updateNegQuality();
            Debug.Log("go=" + go);
            if (!go)
            {
                break;
            }
            measuringTime--;
        }
        SavePositions();
        //avatars.Clear();
        isMeasuring = false;
    }
}
*/