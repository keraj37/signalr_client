﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using WebSocketSharp;
using SimpleJSON;

public class SignalRClient
{
    private WebSocket _ws;
    private string _connectionToken;
    private Dictionary<string, UnTypedActionContainer> _actionMap;

    private string _socketUrl = "http://quisutdeus.in/";
    private string _hubName = "generalhub";
    private string _socket = "ws://quisutdeus.in/";

    public SignalRClient(string url, string hubName)
    {
        _socketUrl = url;
        _socket = _socketUrl.Replace("http", "ws");
        _hubName = hubName;

        //https://quisutdeus.in/signalr/negotiate?clientProtocol=1.5&connectionData=%5B%7B%22name%22%3A%22generalhub%22%7D%2C%7B%22name%22%3A%22webcamhub%22%7D%5D&_=1498628775478
        _actionMap = new Dictionary<string, UnTypedActionContainer>();
        var webRequest = (HttpWebRequest)WebRequest.Create(_socketUrl + "signalr/negotiate?clientProtocol=1.5&connectionData=%5B%7B%22name%22%3A%22generalhub%22%7D%2C%7B%22name%22%3A%22webcamhub%22%7D%5D&_=1498628775478");
        var response = (HttpWebResponse)webRequest.GetResponse();

        using (var sr = new StreamReader(response.GetResponseStream()))
        {
            var payload = sr.ReadToEnd();

            UnityEngine.Debug.Log(payload);

            _connectionToken = Uri.EscapeDataString(JSON.Parse(payload).AsObject["ConnectionToken"].Value);
        }
    }

    public void Open()
    {
        //https://quisutdeus.in/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken=uIU%2BmtFp6asnWlk1w2PpxtAFabrAtJLWWeANBpWs2iWkQhiKhGTT%2FQ7cdPArMnP8bAwYjX45X3JyE3Inr5DafNIkbk5%2F0FDXiS67W0SeXhrjTHdn8%2F8cp36lOmXESrXb&connectionData=%5B%7B%22name%22%3A%22generalhub%22%7D%2C%7B%22name%22%3A%22webcamhub%22%7D%5D&tid=9

        _ws = _ws == null
            ? new WebSocket(_socket + string.Format("signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken={0}&connectionData=%5B%7B%22name%22%3A%22generalhub%22%7D%2C%7B%22name%22%3A%22webcamhub%22%7D%5D&tid=9", _connectionToken))
            : new WebSocket(_socket + string.Format("signalr/reconnect?transport=webSockets&clientProtocol=1.5&connectionToken={0}&connectionData=%5B%7B%22name%22%3A%22generalhub%22%7D%2C%7B%22name%22%3A%22webcamhub%22%7D%5D&tid=9", _connectionToken));

        AttachAndConnect();
    }

    public void Close()
    {
        _ws.Close();
    }

    public void SendMessage(string name, string message, string method = "SendChatMessage")
    {
        var payload = new RollerBallWrapper()
        {
            H = "ChatHub",
            M = method,
            A = new[] { name, message },
            I = 12
        };

        var wsPacket = JsonConvert.SerializeObject(payload);

        _ws.Send(wsPacket);
    }

    public void SendImage(string name, string image, string method = "UpdateWebCamStream")
    {
        var payload = new RollerBallWrapper()
        {
            H = _hubName,
            M = method,
            A = new[] { name, image },
            I = 12
        };

        var wsPacket = JsonConvert.SerializeObject(payload);

        UnityEngine.Debug.Log(wsPacket.ToString());

        _ws.Send(wsPacket);
    }

    private void AttachAndConnect()
    {
        _ws.OnClose += _ws_OnClose;

        _ws.OnError += _ws_OnError;

        _ws.OnMessage += _ws_OnMessage;

        _ws.OnOpen += _ws_OnOpen;

        _ws.Connect();
    }

    void _ws_OnOpen(object sender, EventArgs e)
    {
        UnityEngine.Debug.Log("Opened Connection");
    }

    void _ws_OnMessage(object sender, MessageEventArgs e)
    {
        UnityEngine.Debug.Log(e.Data);
    }

    void _ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        UnityEngine.Debug.Log(e.Message);
    }

    void _ws_OnClose(object sender, CloseEventArgs e)
    {
        UnityEngine.Debug.Log(e.Reason + " Code: " + e.Code + " WasClean: " + e.WasClean);
    }

    public void On<T>(string method, Action<T> callback) where T : class
    {
        _actionMap.Add(method, new UnTypedActionContainer
        {
            Action = new Action<object>(x =>
            {
                callback(x as T);
            }),
            ActionType = typeof(T)
        });
    }
}

internal class UnTypedActionContainer
{
    public Action<object> Action { get; set; }
    public Type ActionType { get; set; }
}

class MessageWrapper
{
    public string C { get; set; }

    public RollerBallWrapper[] M { get; set; }
}

class RollerBallWrapper
{
    public string H { get; set; }

    public string M { get; set; }

    public string[] A { get; set; }

    public int I { get; set; }
}
