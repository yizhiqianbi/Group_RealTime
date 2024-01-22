using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Depth : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera m_Camera;
    public RenderTexture depthTexture, source,colorfulObjTexture, smallDepthTexture;
    public Material Mat;
    public int cameraWidth;
    public int cameraHeight;
    //public int smallCameraWidth;
    //public int smallCameraHeight;

    void Awake()
    {
        cameraWidth = 1024;
        cameraHeight = 1024;
        //smallCameraWidth = 128;
        //smallCameraHeight = 128;
        m_Camera = gameObject.GetComponent<Camera>();
        // 手动设置相机，让它提供场景的深度信息
        // 这样我们就可以在shader中访问_CameraDepthTexture来获取保存的场景的深度信息
        // float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, uv)); 获取某个像素的深度值
        m_Camera.depthTextureMode = DepthTextureMode.Depth;
        /*depthTexture = new RenderTexture(Screen.width, Screen.height, 32,RenderTextureFormat.ARGBFloat);
        fullTexture = new RenderTexture(Screen.width, Screen.height, 32, RenderTextureFormat.ARGBFloat);*/
        depthTexture = new RenderTexture(cameraWidth, cameraHeight, 32, RenderTextureFormat.ARGBFloat);
        colorfulObjTexture = new RenderTexture(cameraWidth, cameraHeight, 32, RenderTextureFormat.ARGBFloat);
        //smallDepthTexture = new RenderTexture(smallCameraWidth, smallCameraHeight, 32, RenderTextureFormat.ARGBFloat);
    }

    
    void OnPostRender()
    {
        source = m_Camera.activeTexture;
        Graphics.Blit(source, colorfulObjTexture);
        Graphics.Blit(source, depthTexture, Mat);
        //Debug.Log("wo shi?");


        //Graphics.Blit(source, smallDepthTexture, Mat);
        //SaveImg()
        //Graphics.Blit(source, depthTexture);
        //Debug.Log("source:"+ source);
        //saveDepthMap(depthTexture, "depth.png");
    }

    public void save() { 
        saveDepthMap(source, this.name + "_scene.jpg");
        // saveDepthMap(m_Camera.targetTexture, this.name + "_scene.jpg");
        saveDepthMap(depthTexture,  this.name + "_depth.jpg");
    }


    // Event function that Unity calls after a Camera has finished rendering, that allows you to modify the Camera's final image.
    /*
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (null != Mat)
        {
            // Copies source texture into destination render texture with a shader.
            // 使用material把这个source渲染到那个destination, Blit(source, destination, material)
            // 使用这个material的意思是使用这个material的shader
            // 这个material没必要赋到物体上
            // Graphics.Blit(source, destination, Mat);
            RenderTexture depthTexture = new RenderTexture(source.width, source.height, 32);
            Graphics.Blit(source, depthTexture, Mat);
            saveDepthMap(depthTexture);
        }
    }
    */

    private void saveDepthMap(RenderTexture DepthRenderTexture, string MapName)
    {
        int Width = DepthRenderTexture.width;
        int Height = DepthRenderTexture.height;
        Texture2D texture2D = new Texture2D(Width, Height);
        var previous = RenderTexture.active;
        RenderTexture.active = DepthRenderTexture;
        texture2D.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        RenderTexture.active = previous;
        texture2D.Apply();
        byte[] Data = texture2D.EncodeToPNG();
        FileStream file = File.Open(MapName, FileMode.Create, FileAccess.Write);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(Data);
        file.Close();
    }

    public void SaveImg(string directory, string fileName)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        var oldT = RenderTexture.active;
        var renderTextureTmp = RenderTexture.GetTemporary(colorfulObjTexture.width, colorfulObjTexture.height, 32);
        RenderTexture.active = m_Camera.targetTexture = renderTextureTmp;
        m_Camera.Render();
        var tmpTexture2D = new Texture2D(m_Camera.targetTexture.width, m_Camera.targetTexture.height);
        tmpTexture2D.ReadPixels(new Rect(0, 0, m_Camera.targetTexture.width, m_Camera.targetTexture.height), 0, 0);
        tmpTexture2D.Apply();
        File.WriteAllBytes(directory + "/" + fileName, tmpTexture2D.EncodeToPNG());
        Destroy(tmpTexture2D);
        RenderTexture.active = oldT;
        m_Camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTextureTmp);
    }
}