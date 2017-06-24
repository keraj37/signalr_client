using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public RawImage img;
    WebCamTexture webCamTexture;

    void Start()
    {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
        img.texture = webCamTexture;
    }

    public void TakeScreenshot()
    {
        if (webCamTexture.isPlaying)
        {
            /*
            Texture2D PhotoTaken = new Texture2D(webCamTexture.width, webCamTexture.height);
            PhotoTaken.SetPixels(webCamTexture.GetPixels());
            PhotoTaken.Apply();

            img.texture = PhotoTaken;
            */
        }
    }
}

 