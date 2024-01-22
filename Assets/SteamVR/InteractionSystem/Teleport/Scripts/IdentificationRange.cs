using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Quality
{
    public double size_quality;
    public double depth_quality;
    public double colorfulness_quality;
    public double covered_quality;
    public double integrity_quality;
    public Quality(double s_q, double d_q, double col_q, double cov_q, double i_q)
    {
        size_quality = s_q;
        depth_quality = d_q;
        colorfulness_quality = col_q;
        covered_quality = cov_q;
        integrity_quality = i_q;
    }
}
public class IdentificationRange : MonoBehaviour
{
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

    public float range;
    public Material material;
    private LineRenderer lineRenderer;
    //private Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, double>>>> recommendedFormation;
    private Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>>> recommendedFormation;
    public void ShowSelected()
    {
        //alineRenderer.enabled = true;
    }

    public void ShowUnSelected()
    {
        lineRenderer.enabled = false;
    }

    public List<Tuple<double,double,double,Quality, double>> GetRecommendedFormation(List<Tuple<Transform, Preference>> avatars)
    {
        //1.获取所有的匹配队形
        //Dictionary<List<Preference>, List<Tuple<double, double, double, double>>> rf = recommendedFormation[avatars.Count];
        Dictionary<List<Preference>, List<Tuple<double, double, double, Quality,double>>> rf = recommendedFormation[avatars.Count];
        //2.排列组合
        //List<Tuple<double, double, double, double>> formation = null;
        List<Tuple<double, double, double, Quality,double>> formation = null;
        double angleOffset3; 
        double minAngleOffset3 = 540;
        Preference p0 = avatars[0].Item2; Preference p1 = avatars[1].Item2; Preference p2 = avatars[2].Item2;
        //0 1 2
        List<Preference> preferences = new List<Preference>() { p0, p1, p2 };
        List<Tuple<double, double, double, Quality,double>> formation0 = GetMatchedFormaiton(rf, preferences);
        angleOffset3 = GetAngleOffset(formation0, avatars);
        if(angleOffset3 < minAngleOffset3)
        {
            minAngleOffset3 = angleOffset3;
            formation = formation0;
        }
        //0 2 1
        preferences = new List<Preference>() { p0, p2, p1 };
        List<Tuple<double, double, double, Quality, double>> formation1 = GetMatchedFormaiton(rf, preferences);
        Exchange(formation1, 1, 2);
        angleOffset3 = GetAngleOffset(formation1, avatars);
        if (angleOffset3 < minAngleOffset3)
        {
            minAngleOffset3 = angleOffset3;
            formation = formation1;
        }
        //1 0 2
        preferences = new List<Preference>() { p1, p0, p2 };
        List<Tuple<double, double, double, Quality, double>> formation2 = GetMatchedFormaiton(rf, preferences);
        Exchange(formation2, 0, 1);
        angleOffset3 = GetAngleOffset(formation2, avatars);
        if (angleOffset3 < minAngleOffset3)
        {
            minAngleOffset3 = angleOffset3;
            formation = formation2;
        }
        //1 2 0
        preferences = new List<Preference>() { p1, p2, p0 };
        List<Tuple<double, double, double, Quality, double>> formation3 = GetMatchedFormaiton(rf, preferences);
        Exchange(formation3, 1, 2);
        Exchange(formation3, 0, 1);
        angleOffset3 = GetAngleOffset(formation3, avatars);
        if (angleOffset3 < minAngleOffset3)
        {
            minAngleOffset3 = angleOffset3;
            formation = formation3;
        }
        //2 0 1
        preferences = new List<Preference>() { p2, p0, p1 };
        List<Tuple<double, double, double, Quality, double>> formation4 = GetMatchedFormaiton(rf, preferences);
        Exchange(formation4, 0, 1);
        Exchange(formation4, 1, 2);
        angleOffset3 = GetAngleOffset(formation4, avatars);
        if (angleOffset3 < minAngleOffset3)
        {
            minAngleOffset3 = angleOffset3;
            formation = formation4;
        }
        //2 1 0
        preferences = new List<Preference>() { p2, p1, p0 };
        List<Tuple<double, double, double, Quality, double>> formation5 = GetMatchedFormaiton(rf, preferences);
        Exchange(formation5, 0, 2);
        angleOffset3 = GetAngleOffset(formation5, avatars);
        if (angleOffset3 < minAngleOffset3)
        {
            minAngleOffset3 = angleOffset3;
            formation = formation5;
        }
        //计算其他人的最小偏移角度
        return getFormationWithMAO(formation, avatars);
    }

    List<Tuple<double, double, double, Quality,double>> GetMatchedFormaiton(Dictionary<List<Preference>, List<Tuple<double, double, double, Quality,double>>> rf, List<Preference> preferences)
    {
        //preferences的长度只有3，只需要匹配前3项
        foreach(var key in rf.Keys)
        {
            if(key[0] == preferences[0] && key[1] == preferences[1] && key[2] == preferences[2])
            {
                //return rf[key];
                return CopyFormation(rf[key]);
            }
        }
        return null;
    }

    List<Tuple<double, double, double, Quality, double>> CopyFormation(List<Tuple<double, double, double, Quality, double>> formation)
    {
        List<Tuple<double, double, double, Quality, double>> copy = new List<Tuple<double, double, double, Quality, double>>();
        foreach(var v in formation)
        {
            Quality q_copy = new Quality(v.Item4.size_quality,v.Item4.depth_quality,v.Item4.colorfulness_quality,v.Item4.covered_quality,v.Item4.integrity_quality);
            Tuple<double, double, double, Quality, double> v_copy = new Tuple<double, double, double, Quality, double>(v.Item1, v.Item2, v.Item3, q_copy, v.Item5);
            copy.Add(v_copy);
        }
        return copy;
    }

    void Exchange(List<Tuple<double, double, double, Quality, double>> formation, int i, int j)
    {
        Tuple<double, double, double, Quality, double> temp = formation[i];
        formation[i] = formation[j];
        formation[j] = temp;
    }

    double GetAngleOffset(List<Tuple<double, double, double, Quality, double>> formation, List<Tuple<Transform, Preference>> avatars)
    {
        //formation的前3和avatars的前3
        Vector3 position = transform.position;
        double angleOffset = 0;
        for(int i = 0; i < 3; i++)
        {
            Vector3 b = new Vector3(position.x - (float)formation[i].Item1, 0, position.z - (float)formation[i].Item3);
            Vector3 a = avatars[i].Item1.forward;
            angleOffset += Mathf.Acos(Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b))) * Mathf.Rad2Deg;
        }
        return angleOffset;
    }

    List<Tuple<double, double, double, Quality, double>> getFormationWithMAO(List<Tuple<double, double, double, Quality, double>> formation, List<Tuple<Transform, Preference>> avatars)
    {
        //formation的前3已经匹配完成，要求匹配后面的位置，返回匹配完成的formation
        //1.初始化数据
        //2.调用算法获取最小偏转角
        //3.返回结果

        //1.初始化数据
        _n = formation.Count - 3;
        _offsets = new double[_n, _n];
        _destinations = new int[_n];
        _destinations_record = new int[_n];
        _minOffset = _n * 180;//设置很大的初始值
        Vector3 position = transform.position;
        for (int i = 0; i < _n; i++)
        {
            for(int j = 0; j < _n; j++)
            {
                //第i个人到第j个位置时的偏转角，其中第i,j都是从第四个开始，因为前三已经完成了匹配
                Vector3 a = avatars[i + 3].Item1.forward;
                Vector3 b = new Vector3(position.x - (float)formation[j + 3].Item1, 0, position.z - (float)formation[j + 3].Item3);
                _offsets[i, j] = Mathf.Acos(Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b))) * Mathf.Rad2Deg;
            }
            _destinations[i] = -1;//-1表示未匹配
            _destinations_record[i] = -1;
        }
        //2.调用算法获取最小偏转角
        //太慢了
        //_Backstrack(0, 0);// 第一个0表示从第一个人开始 ，第二个0表示当前总偏转
        //贪心
        for(int i = 0; i < _n; i++)
        {
            double min = 180;
            int min_position = -1;
            for(int j = 0; j < _n; j++)
            {
                if(_destinations_record[j] == -1)
                {
                    if(_offsets[i,j] < min)
                    {
                        min = _offsets[i, j];
                        min_position = j;
                    }
                }
            }
            _destinations_record[min_position] = i;
        }
        //3.返回结果，将_destinations中保存的结果以正确形式返回
        List<Tuple<double, double, double, Quality, double>> resultFormation = new List<Tuple<double, double, double, Quality, double>>(formation.Count);
        resultFormation.Add(formation[0]);
        resultFormation.Add(formation[1]);
        resultFormation.Add(formation[2]);
        for(int i = 0; i < _n; i++)
        {
            resultFormation.Add(null);
        }
        for(int i = 0; i < _n; i++)
        {
            resultFormation[_destinations_record[i] + 3] = formation[i + 3];
        }
        return resultFormation;
    }


    double[,] _offsets;
    int[] _destinations;//_destination[i]的值表示匹配第i个位置的用户index
    int[] _destinations_record;
    int _n;
    double _minOffset;
    void _Backstrack(int i, double o)
    {
        if (o > _minOffset) return;//如果目前偏转已经很大了，则不用继续算
        if (i == _n)//当最后一个人也匹配好后判断总偏转
        {
            if (o < _minOffset) _minOffset = o;
            for (int j = 0; j < _destinations.Length; j++)
            {
                _destinations_record[j] = _destinations[j];
            }
            return;
        }
        for (int j = 0; j < _n; j++)
        {
            if (_destinations[j] == -1)//如果第j个位置还没有匹配
            {
                _destinations[j] = i;
                _Backstrack(i + 1, o + _offsets[i, j]);
                _destinations[j] = -1;
            }
        }
    }


    void ReadRecommendedFormation()
    {
        recommendedFormation = new Dictionary<int, Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>>>();
        string directory = Application.dataPath + "/Resources/RecommendedFormation";
        if (!Directory.Exists(directory))
        {
            Debug.Log("推荐队形不存在，请先生成推荐队形：" + directory);
            return;
        }
        List<int> groupScales = new List<int>() { 5, 10, 15 };
        foreach(int groupScale in groupScales)
        {
            string path = directory + "/" + this.gameObject.name + "_" + groupScale + ".json";
            if (!File.Exists(path))
            {
                Debug.Log("推荐队形不存在，请先生成推荐队形：" + path);
            }
            else
            {
                recommendedFormation[groupScale] = new Dictionary<List<Preference>, List<Tuple<double, double, double, Quality, double>>>();
                string jsonFromFile = File.ReadAllText(path);
                FormationData formationData = JsonUtility.FromJson<FormationData>(jsonFromFile);
                foreach(OneFormation oneFormation in formationData.formations)
                {
                    List<Preference> preferences = new List<Preference>();
                    List<Tuple<double, double, double, Quality, double>> positions = new List<Tuple<double, double, double, Quality, double>>();
                    for(int i = 0; i < groupScale; i++)
                    {
                        preferences.Add((Preference)oneFormation.preferences[i]);
                        double x = oneFormation.xs[i];
                        double y = oneFormation.ys[i];
                        double z = oneFormation.zs[i];
                        double s_q = oneFormation.sizeQs[i];
                        double d_q = oneFormation.depthQs[i];
                        double col_q = oneFormation.colorQs[i];
                        double cov_q = oneFormation.coverQs[i];
                        double i_q = oneFormation.intQs[i];
                        double f_q = oneFormation.finalQs[i];
                        Quality quality = new Quality(s_q, d_q, col_q, cov_q, i_q);
                        Tuple<double, double, double, Quality, double> position = new Tuple<double, double, double, Quality, double>(x, y, z, quality, f_q);
                        positions.Add(position);
                    }
                    recommendedFormation[groupScale][preferences] = positions;
                }
            }
        }
    }

    public void Awake()
    {
        if (!lineRenderer)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = material;
        }
        Vector3 center = transform.position;
        float radius = range;
        int cnt = 90;
        lineRenderer.positionCount = cnt;
        lineRenderer.startWidth = .02f;
        lineRenderer.endWidth = .02f;
        for (int i = 0; i < cnt; i++)
        {
            float x = center.x + radius * Mathf.Cos(i * 360 / cnt * Mathf.PI / 180f);
            float z = center.z + radius * Mathf.Sin(i * 360 / cnt * Mathf.PI / 180f);
            lineRenderer.SetPosition(i, new Vector3(x, transform.position.y, z));
        }
        lineRenderer.enabled = false;
/*        ReadRecommendedFormation();//加载推荐队形
        Debug.Log("加载完毕");*/
    }
}
