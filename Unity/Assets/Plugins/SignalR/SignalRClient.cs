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

public enum RemoteCommand
{
    SCREENSHOT,
    START_STREAM,
    STOP_STREAM,
    SET_STREAM_DELAY
}

public class SignalRClient
{
    private WebSocket _ws;
    private string _connectionToken;

    private string _socketUrl = "";
    private string _hubName = "";
    private string _socket = "";
    private System.Action<ConnectionStatus> onConnectionUpdate;
    private System.Action<RemoteCommand, string> onRemoteCommand;

    public SignalRClient(string url, string hubName, System.Action<ConnectionStatus> onConnectionUpdate, System.Action<RemoteCommand, string> onRemoteCommand)
    {
        this.onConnectionUpdate = onConnectionUpdate;
        this.onRemoteCommand = onRemoteCommand;
        _socketUrl = "http://" + url;
        _socket = "ws://" + url;
        _hubName = hubName;

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
        var payload = new SignalRMessage()
        {
            H = _hubName,
            M = method,
            A = new[] { name, image },
            I = 12
        };

        var wsPacket = JsonConvert.SerializeObject(payload);

        _ws.Send(wsPacket);
    }

    public void SendConnected(string name, string device, string method = "DevicePing")
    {
        var payload = new SignalRMessage()
        {
            H = _hubName,
            M = method,
            A = new[] { name, device },
            I = 12
        };

        var wsPacket = JsonConvert.SerializeObject(payload);

        _ws.Send(wsPacket);
    }

    public void SendDisconnected(string name, string method = "DeviceDisconnected")
    {
        var payload = new SignalRMessage()
        {
            H = _hubName,
            M = method,
            A = new[] { name },
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

#if UNITY_EDITOR
        UnityEngine.Debug.Log(e.Data);
#endif

        if (json.ContainsKey("E"))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.ERROR));
        }
        else
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => onConnectionUpdate(ConnectionStatus.CONNECTED));

            var msgs = SignalRMessage.FromJSON(json);
            foreach (var msg in msgs)
            {
                switch(msg.M)
                {
                    case "remoteCommand":
                        RemoteCommand cmd = (RemoteCommand)System.Enum.Parse(typeof(RemoteCommand), msg.A[0].ToUpper());
                        var param = msg.A.Length > 1 ? msg.A[1] : string.Empty;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => onRemoteCommand(cmd, param));
                        break;
                }
            }
        }
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
}

class SignalRMessage
{
    public string H { get; set; }

    public string M { get; set; }

    public string[] A { get; set; }

    public int I { get; set; }

    public static SignalRMessage[] FromJSON(JSONClass json)
    {
        List<SignalRMessage> result = new List<SignalRMessage>();

        if (json.ContainsKey("M") && json["M"].AsArray != null)
        {
            foreach (JSONNode node in json["M"].AsArray)
            {
                SignalRMessage msg = new SignalRMessage();
                msg.H = node.AsObject.ContainsKey("H") ? node.AsObject["H"].Value : string.Empty;
                msg.M = node.AsObject.ContainsKey("M") ? node.AsObject["M"].Value : string.Empty;

                if (node.AsObject.ContainsKey("A") && node.AsObject["A"].AsArray != null)
                {
                    List<string> paramsA = new List<string>();

                    foreach (JSONNode nodeParam in node.AsObject["A"].AsArray)
                    {
                        paramsA.Add(nodeParam.Value);
                    }

                    msg.A = paramsA.ToArray();
                }

                result.Add(msg);
            }
        }

        return result.ToArray();
    }
}
