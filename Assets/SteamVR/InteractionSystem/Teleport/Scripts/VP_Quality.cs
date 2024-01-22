using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

/*public class Quality
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
}*/

public class PreQuality
{
    public double size_quality;
    public double depth_quality;
    public double colorfulness_quality;
    public double integrity_quality;
    public PreQuality(double s_q, double d_q, double col_q, double i_q)
    {
        size_quality = s_q;
        depth_quality = d_q;
        colorfulness_quality = col_q;
        integrity_quality = i_q;
    }
}

public class VP_Quality : MonoBehaviour
{
    struct FastData {
        public int ObjArea;
        public int CoveredArea;
        public int LargeArea;
        public float rg;
        public float rg2;
        public float yb;
        public float yb2;
        public float depth;
        public float depth2;
    }

    public ComputeShader fastShader;
    private RenderTexture area_outputBuffer;
    private RenderTexture color_outputBuffer;
    private RenderTexture depth_outputBuffer;

    public ComputeShader secondFastShader;
    private ComputeBuffer secondFastOutputbuffer;
    private FastData[] secondFastOutputData;


    private Depth allDepth, objDepth,largeDepth;
    private int width, height, smallWidth, smallHeight;
    private double proportion;
    [Header("Debug")]
    public int viewArea;
    public int objArea;
    public int coveredArea;
    public int largeArea;
    public float size_quality;
    public float covered_quality;
    public float integrity_quality;
    public double depthVariance_quality;
    public double colorfulness_quality;
    public double final_quality;
    public bool checkImg;

    Stopwatch sw;
    void Start()
    {
        allDepth = transform.Find("RenderAllCamera").GetComponent<Depth>();
        objDepth = transform.Find("RenderObjCamera").GetComponent<Depth>();
        largeDepth = transform.Find("RenderLargeCamera").GetComponent<Depth>();
        width = allDepth.cameraWidth;
        height = allDepth.cameraHeight;
        double main_fov = transform.Find("RenderObjCamera").GetComponent<Camera>().fieldOfView / 2 / 180 * Math.PI;
        double large_fov = transform.Find("RenderLargeCamera").GetComponent<Camera>().fieldOfView / 2 / 180 * Math.PI;
        proportion = Math.Tan(large_fov) / Math.Tan(main_fov);
        proportion = Math.Pow(proportion, 2.0);

        //二级
        area_outputBuffer = new RenderTexture(width / 8, height / 8, 32, RenderTextureFormat.ARGBFloat) { enableRandomWrite = true };
        color_outputBuffer = new RenderTexture(width / 8, height / 8, 32, RenderTextureFormat.ARGBFloat) { enableRandomWrite = true };
        depth_outputBuffer = new RenderTexture(width / 8, height / 8, 32, RenderTextureFormat.ARGBFloat) { enableRandomWrite = true };
        area_outputBuffer.Create();
        color_outputBuffer.Create();
        depth_outputBuffer.Create();
        secondFastOutputData = new FastData[width / 8 / 8 * height / 8 / 8];
        secondFastOutputbuffer = new ComputeBuffer(secondFastOutputData.Length, 9 * 4);
    }

    void Update()
    {

    }
    public Quality getQuality()
    {
        //一级
        fastShader.SetTexture(0, "AllDepthTex", allDepth.depthTexture);
        fastShader.SetTexture(0, "ObjDepthTex", objDepth.depthTexture);
        fastShader.SetTexture(0, "LargeDepthTex", largeDepth.depthTexture);
        fastShader.SetTexture(0, "ColorfulObjTexture", objDepth.colorfulObjTexture);
        fastShader.SetTexture(0, "area_outputBuffer", area_outputBuffer);
        fastShader.SetTexture(0, "color_outputBuffer", color_outputBuffer);
        fastShader.SetTexture(0, "depth_outputBuffer", depth_outputBuffer);
        fastShader.Dispatch(0, allDepth.depthTexture.width / 8, allDepth.depthTexture.height / 8, 1);
        //二级
        secondFastShader.SetTexture(0, "area_inputBuffer", area_outputBuffer);
        secondFastShader.SetTexture(0, "color_inputBuffer", color_outputBuffer);
        secondFastShader.SetTexture(0, "depth_inputBuffer", depth_outputBuffer);
        secondFastShader.SetBuffer(0, "outputData", secondFastOutputbuffer);
        secondFastShader.Dispatch(0, area_outputBuffer.width / 8, area_outputBuffer.height / 8, 1);
        secondFastOutputbuffer.GetData(secondFastOutputData);
        FastData[] outputData = secondFastOutputData;

        objArea = coveredArea = largeArea = 0;
        viewArea = allDepth.depthTexture.width * allDepth.depthTexture.height;
        double rgSum = 0, ybSum = 0, rg2Sum = 0, yb2Sum = 0;
        double rgMean, rgVar, ybMean, ybVar, rg2Mean, yb2Mean, stdRoot, meanRoot;
        double depthSum = 0, depth2Sum = 0;
        double depthMean, depth2Mean, depthVar;
        int length = width * height;
        for (int i = 0; i < outputData.Length; i++)
        {
            FastData fastData = outputData[i];
            objArea += fastData.ObjArea;
            coveredArea += fastData.CoveredArea;
            largeArea += fastData.LargeArea;
            rgSum += fastData.rg;
            rg2Sum += fastData.rg2;
            ybSum += fastData.yb;
            yb2Sum += fastData.yb2;
            depthSum += fastData.depth;
            depth2Sum += fastData.depth2;
        }
        rgMean = rgSum / length;
        ybMean = ybSum / length;
        rg2Mean = rg2Sum / length;
        yb2Mean = yb2Sum / length;
        rgVar = rg2Mean - Math.Pow(rgMean, 2);
        ybVar = yb2Mean - Math.Pow(ybMean, 2);
        stdRoot = Math.Sqrt(rgVar + ybVar);
        meanRoot = Math.Sqrt(Math.Pow(rgMean, 2) + Math.Pow(ybMean, 2));
        
        depthMean = depthSum / length;
        depth2Mean = depth2Sum / length;
        depthVar = depth2Mean - Math.Pow(depthMean, 2);

        size_quality = (float)objArea / viewArea;//在视野中的面积占比
        if(objArea == 0) { covered_quality = 0; } else { covered_quality = 1 - (float)coveredArea / objArea; }//1-被（人）遮挡的比例
        if(largeArea == 0) { integrity_quality = 0; } else { integrity_quality = (float)objArea / (int)(proportion * largeArea); }//视野中的展品完整度
        colorfulness_quality = stdRoot + 0.3 * meanRoot;//色彩丰富度
        depthVariance_quality = Math.Sqrt(depthVar);//深度丰富度
        if(colorfulness_quality > 1)  colorfulness_quality = 0.0984432828862205f; //仅针对地球展品中某个点出现的问题，这里最好判定一下，但是你妈的不一定能复现
        Quality quality = new Quality(size_quality, depthVariance_quality, colorfulness_quality, covered_quality, integrity_quality);
        if (checkImg) SaveImg(quality);
        return quality;
    }

    void SaveImg(Quality quality)
    {
        string directory = Application.dataPath + "/Resources/CheckImg";
        SaveAllDepthImg(directory, DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + "_ALL_RAWDATA_s_q" + quality.size_quality + ",i_q" + quality.integrity_quality + ",cov_q" + quality.covered_quality + ",col_q" + quality.colorfulness_quality + ",d_q" + quality.depth_quality + ".png");
        SaveObjDepthImg(directory, DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + "_OBJ_RAWDATA_s_q" + quality.size_quality + ",i_q" + quality.integrity_quality + ",cov_q" + quality.covered_quality + ",col_q" + quality.colorfulness_quality + ",d_q" + quality.depth_quality + ".png");
    }

    public PreQuality getPreQuality()
    {
        Quality quality = getQuality();
        PreQuality preQuality = new PreQuality(quality.size_quality, quality.depth_quality, quality.colorfulness_quality, quality.integrity_quality);
        return preQuality;
    }

    public double getCoveredQuality()
    {
        return 0;
    }



    public void SaveAllDepthImg(string directory, string fileName)
    {
        allDepth.SaveImg(directory, fileName);
    }

    public void SaveObjDepthImg(string directory, string fileName)
    {
        objDepth.SaveImg(directory, fileName);
    }
}
