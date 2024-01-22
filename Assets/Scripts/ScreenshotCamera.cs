using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotCamera : MonoBehaviour
{
    public int width = 2048;
    public int height = 2048;
    public bool png;
    public void SaveImg()
    {
        Camera camera = GetComponent<Camera>();
        string directory = Application.dataPath + "/Resources/Screenshot";
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string fileName;
        if (png)
        {
            fileName = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + ".png";
        }
        else
        {
            fileName = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss") + ".jpg";
        }
        var oldT = RenderTexture.active;
        var renderTextureTmp = RenderTexture.GetTemporary(width, height, 32);
        RenderTexture.active = camera.targetTexture = renderTextureTmp;
        camera.Render();
        var tmpTexture2D = new Texture2D(width, height);
        tmpTexture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tmpTexture2D.Apply();
        if (png)
        {
            File.WriteAllBytes(directory + "/" + fileName, tmpTexture2D.EncodeToPNG());
        }
        else
        {
            File.WriteAllBytes(directory + "/" + fileName, tmpTexture2D.EncodeToJPG(100));
        }
        
        Destroy(tmpTexture2D);
        RenderTexture.active = oldT;
        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTextureTmp);

        Debug.Log("SaveImg Successfully to " + directory + "/" + fileName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveImg();
        }
    }
}
