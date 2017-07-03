using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using WebSocketSharp;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine;

public enum ConnectionStatus
{
    CONNECTING,
    CONNECTED,
    NOT_CONNECTED,
    DISCONNECTED,
    ERROR
}

public class SignalRClient
{
    private WebSocket _ws;
    private string _connectionToken;
    private Dictionary<string, UnTypedActionContainer> _actionMap;

    private string _socketUrl = "http://";
    private string _hubName = "generalhub";
    private string _socket = "ws://";
    private System.Action<ConnectionStatus> onConnectionUpdate;

    public SignalRClient(string url, string hubName, System.Action<ConnectionStatus> onConnectionUpdate)
    {
        this.onConnectionUpdate = onConnectionUpdate;
        _socketUrl = "http://" + url;
        _socket = "ws://" + url;
        _hubName = hubName;        

        _actionMap = new Dictionary<string, UnTypedActionContainer>();

        TryConnect();
    }

    public void TryConnect()
    {
        onConnectionUpdate(ConnectionStatus.CONNECTING);

        try
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(_socketUrl + string.Format("/signalr/negotiate?clientProtocol=1.5&connectionData=%5B%7B%22name%22%3A%22{0}%22%7D%5D&_=1498643185939", _hubName));
            var response = (HttpWebResponse)webRequest.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                var payload = sr.ReadToEnd();

                JSONClass json = JSON.Parse(payload).AsObject;

                if (json != null && json.ContainsKey("ConnectionToken"))
                {
                    _connectionToken = Uri.EscapeDataString(json["ConnectionToken"].Value);
                    onConnectionUpdate(ConnectionStatus.CONNECTED);
                }
                else
                {
                    onConnectionUpdate(ConnectionStatus.NOT_CONNECTED);
                }
            }

            Open();
        }
        catch (Exception e)
        {
            onConnectionUpdate(ConnectionStatus.NOT_CONNECTED);
        }
    }

    public void Open()
    {
        _ws = _ws == null
            ? new WebSocket(_socket + string.Format("/signalr/connect?transport=webSockets&clientProtocol=1.5&connectionToken={0}&connectionData=%5B%7B%22name%22%3A%22{1}%22%7D%5D&tid=8", _connectionToken, _hubName))
            : new WebSocket(_socket + string.Format("/signalr/reconnect?transport=webSockets&clientProtocol=1.5&connectionToken={0}&connectionData=%5B%7B%22name%22%3A%22{1}%22%7D%5D&tid=8", _connectionToken, _hubName));

        AttachAndConnect();
    }

    public void Close()
    {
        _ws.Close();
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
        UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.CONNECTED));

        UnityEngine.Debug.Log("Opened Connection");
    }

    void _ws_OnMessage(object sender, MessageEventArgs e)
    {
        JSONClass json = JSON.Parse(e.Data).AsObject;

        if(json.ContainsKey("E"))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.ERROR));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.CONNECTED));
        }

        UnityEngine.Debug.Log(e.Data);
    }

    void _ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.ERROR));

        UnityEngine.Debug.Log(e.Message);
    }

    void _ws_OnClose(object sender, CloseEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.DISCONNECTED));

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
