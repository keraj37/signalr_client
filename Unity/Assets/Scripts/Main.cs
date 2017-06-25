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
    public InputField inputName;
    public Text streamText;

    WebCamTexture webCamTexture;
    Coroutine uploadCoroutine;

    private bool IsStreaming
    {
        get { return uploadCoroutine != null; }
    }

    void Start()
    {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
        img.texture = webCamTexture;
    }

    public void Test()
    {
        StartCoroutine(Upload("TEST TEST"));
    }

    public void TakeScreenshot()
    {
        if (webCamTexture.isPlaying)
        {
            Texture2D PhotoTaken = new Texture2D(webCamTexture.width, webCamTexture.height);
            PhotoTaken.SetPixels(webCamTexture.GetPixels());
            PhotoTaken.Apply();

            img.texture = PhotoTaken;

            byte[] image = PhotoTaken.EncodeToJPG(50);

            StartCoroutine(Upload(Convert.ToBase64String(image)));

            //File.WriteAllBytes(Application.persistentDataPath + "/test.jpg", image);
        }
    }

    public void StartStream()
    {
        if(IsStreaming)
        {
            streamText.text = "Start Stream";

            StopCoroutine(uploadCoroutine);
            uploadCoroutine = null;
        }
        else
        {
            streamText.text = "Stop Stream";

            img.texture = webCamTexture;
            StreamNext();
        }
    }

    private void StreamNext()
    {
        if (webCamTexture.isPlaying)
        {
            Texture2D PhotoTaken = new Texture2D(webCamTexture.width, webCamTexture.height);
            PhotoTaken.SetPixels(webCamTexture.GetPixels());
            PhotoTaken.Apply();

            byte[] image = PhotoTaken.EncodeToJPG(50);

            uploadCoroutine = StartCoroutine(Upload(Convert.ToBase64String(image), StreamNext));
        }
    }

    IEnumerator Upload(string image, Action onComplete = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", inputName.text);
        form.AddField("image", image);

        Debug.Log("Sending: " + image);

        UnityWebRequest www = UnityWebRequest.Post("https://quisutdeus.in/Projects/WebCam", form);
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }

        if(onComplete != null)
        {
            onComplete();
        }
    }
}

 