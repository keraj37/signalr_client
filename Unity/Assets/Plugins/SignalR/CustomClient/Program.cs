﻿//using Newtonsoft.Json.Linq;
using SignalR.Client._20.Hubs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

public static class Program
{
    public static void Test()
    {
        // uncomment below to stream debug into console
        // Debug.Listeners.Add(new ConsoleTraceListener());

        // this is an optional query parameters to sent with each message
        var query = new Dictionary<string, string>();
        query.Add("version", "1.0");

        // initialize connection and its proxy
        HubConnection connection = new HubConnection("https://quisutdeus.in", query);
        IHubProxy proxy = connection.CreateProxy("GeneralHub");

        // subscribe to event
        proxy.Subscribe("addNewMessageToPage").Data += data =>
        {
                /*
                var _first = data[0] as JToken;
                Debug.Log(string.Format("Received: [{0}] from {1}",
                    _first["message"].ToString(), _first["from"].ToString()));
                    */
        };

        connection.Closed += () => Debug.Log("Closed");
        connection.Error += x => Debug.Log("Error: " + x.Message);
        connection.Received += x => Debug.Log("Recived: " + x);
        connection.Reconnected += () => Debug.Log("Reconected");

        Debug.Log("Connecting... ");

        new Thread(() => connection.Start()).Start();

        Debug.Log("done. Hit: ");
        Debug.Log("1:\tSend hello message");
        Debug.Log("2:\tRequest => Reply with dynamic reply");
        Debug.Log("3:\tRequest => Reply with value type");
        Debug.Log("Esc:\tExit");
        Debug.Log("");

        /*
        Debug.Log("Sending hi... ");
        proxy.Invoke("SendChatMessage", "JS CLient", "I am working").Finished += (sender, e) =>
        {
            Debug.Log("done");
        };
        */
    }
}
