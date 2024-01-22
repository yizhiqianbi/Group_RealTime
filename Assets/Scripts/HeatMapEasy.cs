using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapEasy : MonoBehaviour
{
    private float[,] temperature;
    private int horizontal = 100;
    private int vertical = 100;
    private MeshFilter meshFilter;
    private Vector3[] vertices;
    private Vector2[] uv;

    public float perWidth = 1;
    public float perHeight = 1;
    public float MinTemperature = 20;
    public float MaxTemperature = 100;
    public Color[] TemperatureColors;

    public void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        vertices = new Vector3[horizontal * vertical];
        uv = new Vector2[horizontal * vertical];
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Inject(InitTemperatures());
        }
    }

    // 初始化算法
    private float[,] InitTemperatures()
    {
        // vertical 和 horizontal 均为100
        float[,] temperature = new float[vertical, horizontal];
        for (int i = 0; i < vertical; i++)
            for (int j = 0; j < horizontal; j++)
                // 假定正常温度为50度低于50度均为正常温度
                temperature[i, j] = 50;
        return temperature;
    }

    public void Inject(float[,] temperature)
    {
        this.temperature = temperature;
        this.horizontal = temperature.GetLength(1);
        this.vertical = temperature.GetLength(0);
        // 利用随机产生高温点的算法添加6个高温点
        RandomTeamperature(99, 50, 0, 45, ref this.temperature);
        RandomTeamperature(89, 50, 0, 25, ref this.temperature);
        RandomTeamperature(79, 50, 0, 30, ref this.temperature);
        RandomTeamperature(69, 50, 0, 33, ref this.temperature);
        RandomTeamperature(89, 50, 0, 50, ref this.temperature);
        RandomTeamperature(79, 50, 0, 22, ref this.temperature);
        //meshFilter.mesh = DrawHeatMap();//生成网格，在Grid.cs中已经实现
        // 赋值颜色
        AddVertexColor();
    }

    // 随机在某个点产生高温数据，并按minD和maxD温度范围来生成模拟数据
    // minD和maxD为以当前高温点为中心的生效范围
    // from为最高温度，to为最低温度
    private void RandomTeamperature(float from, float to, int minD, int maxD, ref float[,] temperatures)
    {
        // 产生随机点
        int randomX = Random.Range(3, horizontal);
        int randomY = Random.Range(3, vertical);

        float maxTweenDis = maxD - minD;
        float offset = to - from;
        for (int i = randomX - maxD; i < randomX + maxD; i++)
        {
            for (int j = randomY + maxD; j > randomY - maxD; j--)
            {
                if (i < 0 || i >= horizontal)
                    continue;
                if (j < 0 || j >= vertical)
                    continue;
                float distance = Mathf.Sqrt(Mathf.Pow(randomX - i, 2) + Mathf.Pow(randomY - j, 2));
                if (distance <= maxD && distance >= minD)
                {
                    float offsetDis = distance - minD;
                    float ratio = offsetDis / maxTweenDis;
                    float temp = from + ratio * offset;
                    // 只有比当前点温度高才选择覆盖
                    if (temp > temperatures[i, j])
                        temperatures[i, j] = temp;
                }
            }
        }
    }

    private void AddVertexColor()
    {
        Color[] colors = new Color[meshFilter.mesh.colors.Length];
        for (int j = 0; j < vertical; j++)
        {
            for (int i = 0; i < horizontal; i++) {
                float temperature = this.temperature[j, i];
                // 利用温度值计算顶点颜色值
                colors[horizontal * j + i] = CalcColor(temperature);
                Vector3 vertex = new Vector3(i * perWidth, j * perHeight, GetHeightByTemperature(temperature));
                vertices[horizontal * j + i] = vertex;
                uv[horizontal * j + i] = new Vector2(0, 1) + new Vector2(1 / horizontal * i, 1 / vertical * j);
            }
        }
        meshFilter.mesh.colors = colors;
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.uv = uv;
    }

    private Color CalcColor(float temperature)
    {
        int count = (int)temperature / 10;
        float temp = (temperature % 10) / 10;
        Color[] colors = GetColors(count);
        Color from = colors[0];
        Color to = colors[1];
        Color offset = to - from;
        return from + offset * temp;
    }

    // TemperatureColors 实在Inspector面板中配置的颜色区间

    private Color[] GetColors(int index)
    {
        Color startColor = Color.blue, endColor = Color.blue;
        startColor = TemperatureColors[index];
        endColor = TemperatureColors[index + 1];
        return new Color[] { startColor, endColor };
    }

    private float GetHeightByTemperature(float temperature)
    {
        return (0.5f - (temperature - MinTemperature) / (MaxTemperature - MinTemperature));
    }
}
