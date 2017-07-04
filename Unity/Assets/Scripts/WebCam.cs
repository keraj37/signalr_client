using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

public class WebCam : MonoBehaviour
{
    public const string DOMAIN = "quisutdeus.in";

    public RawImage img;
    public Texture2D imgNull;
    public InputField inputDelay;
    public Text streamText;
    public Text statusTxt;

    public GameObject showWhenConnected;
    public GameObject showWhenNotConnected;

    WebCamTexture webCamTexture;
    SignalRClient ws;
    ConnectionStatus currentStatus = ConnectionStatus.DISCONNECTED;

    private float _delay = 0f;
    private float _delayPing = 0f;

    private bool IsStreaming { get; set; }

    private void Awake()
    {
        showWhenConnected.SetActive(false);
        showWhenNotConnected.SetActive(false);

        Application.runInBackground = true;
    }

    IEnumerator Start()
    {
        webCamTexture = new WebCamTexture(640, 480);
        webCamTexture.Play();
        img.texture = webCamTexture;

        statusTxt.text = "Status: <color=#ffa500ff>Connecting.... </color>";

        yield return new WaitForSecondsRealtime(1f);

        CreateWebSocketInstance();
    }

    public void Exit()
    {
        Disconnect();
        Application.Quit();
    }

    private void Disconnect()
    {
        if(currentStatus == ConnectionStatus.CONNECTED)
            ws.SendDisconnected(Login.LOGGEDIN_NAME);

        currentStatus = ConnectionStatus.NOT_CONNECTED;

        ws.Close();
    }

    private void CreateWebSocketInstance()
    {
        if (ws != null)
            return;

        ws = new SignalRClient(DOMAIN, "GeneralHub", status =>
        {
            showWhenConnected.SetActive(status == ConnectionStatus.CONNECTED);
            showWhenNotConnected.SetActive(status != ConnectionStatus.CONNECTED && status != ConnectionStatus.CONNECTING);

            currentStatus = status;

            switch (status)
            {
                case ConnectionStatus.CONNECTED:                   
                    statusTxt.text = "Status: <color=#00ffffff>Connected.</color>";
                    break;
                case ConnectionStatus.NOT_CONNECTED:
                    IsStreaming = false;
                    statusTxt.text = "Status: <color=#ff0000ff>Connection failed.</color>";
                    break;
                case ConnectionStatus.ERROR:
                    IsStreaming = false;
                    statusTxt.text = "Status: <color=#ff0000ff>Error.</color>";
                    break;
                case ConnectionStatus.DISCONNECTED:
                    IsStreaming = false;
                    statusTxt.text = "Status: <color=#ffa500ff>Disconnected.</color>";
                    break;
                case ConnectionStatus.CONNECTING:
                    statusTxt.text = "Status: <color=#ffa500ff>Connecting.... </color>";
                    break;
            }
        });
    }

    public void Retry()
    {
        showWhenNotConnected.SetActive(false);
        statusTxt.text = "Status: <color=#ffa500ff>Connecting.... </color>";

        StartCoroutine(RetryCoroutine());
    }

    private IEnumerator RetryCoroutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        ws.TryConnect();
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void SendImage()
    {
        if (webCamTexture.isPlaying)
        {
            ws.SendImage(Login.LOGGEDIN_NAME, GetCurrentFrame());
        }
        else
        {
            byte[] image = imgNull.EncodeToJPG(50); 
            ws.SendImage(Login.LOGGEDIN_NAME, Convert.ToBase64String(image));
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
        if(currentStatus == ConnectionStatus.CONNECTED)
        {
            _delayPing -= Time.deltaTime;

            if (_delayPing <= 0f)
            {
                _delayPing = 2f;
                ws.SendConnected(Login.LOGGEDIN_NAME, SystemInfo.deviceModel);
            }
        }

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

 