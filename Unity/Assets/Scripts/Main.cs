using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

public class Main : MonoBehaviour
{
    public RawImage img;
    public Texture2D imgNull;
    public InputField inputName;
    public InputField inputDelay;
    public Text streamText;

    WebCamTexture webCamTexture;
    SignalRClient ws;

    private float _delay = 0f;

    private bool IsStreaming { get; set; }

    void Start()
    {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
        img.texture = webCamTexture;

        ws = new SignalRClient("http://quisutdeus.in/", "GeneralHub");
        ws.Open();
    }

    private void OnDestroy()
    {
        ws.Close();
    }

    public void SendImage()
    {
        if (webCamTexture.isPlaying)
        {
            ws.SendImage(inputName.text, GetCurrentFrame());
        }
        else
        {
            byte[] image = imgNull.EncodeToJPG(50); 
            ws.SendImage(inputName.text, Convert.ToBase64String(image));
        }
    }

    public void StartStream()
    {
        IsStreaming = !IsStreaming;

        if (!IsStreaming)
        {
            streamText.text = "Start Stream";
        }
        else
        {
            float.TryParse(inputDelay.text, out _delay);

            streamText.text = "Stop Stream";
        }
    }

    private string GetCurrentFrame()
    {
        Texture2D photoTaken = new Texture2D(webCamTexture.width, webCamTexture.height);
        photoTaken.SetPixels(webCamTexture.GetPixels());
        photoTaken.Apply();

        byte[] image = photoTaken.EncodeToJPG(50);

        DestroyImmediate(photoTaken);

        return Convert.ToBase64String(image);
    }

    private void Update()
    {
        if(IsStreaming)
        {
            _delay -= Time.deltaTime;

            if (_delay <= 0f)
            {
                float.TryParse(inputDelay.text, out _delay);

                SendImage();
            }
        }
    }
}

 