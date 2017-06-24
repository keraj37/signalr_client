using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using SignalR.Client._20.Hubs;
using System.Threading;

public class Main : MonoBehaviour
{
    HubConnection connection;
    IHubProxy myHub;

    void Start()
    {
        // uncomment below to stream debug into console
        // Debug.Listeners.Add(new ConsoleTraceListener());

        // initialize connection and its proxy
        connection = new HubConnection("https://quisutdeus.in");
        myHub = connection.CreateProxy("GeneralHub");

        myHub.Subscribe("addNewMessageToPage").Data += data =>
        {
            var _first = data[0] as JToken;
            Debug.Log(string.Format("Received: [{0}] from {1}",
                _first["message"].ToString(), _first["from"].ToString()));
        };

        connection.Error += x => Debug.LogError(x.Message);

        new Thread(() =>
        {
            connection.Start();           
        }).Start();

        new Thread(() =>
        {
            myHub.Invoke("SendChatMessage", "JS CLient", "I am working").Finished += (sender, e) =>
            {
                Debug.Log("done");
            };
        }).Start();

        /*
        // subscribe to event
        proxy.Subscribe("Pong").Data += data =>
        {
            var _first = data[0] as JToken;
            Console.WriteLine("Received: [{0}] from {1}",
                _first["message"].ToString(), _first["from"].ToString());
        };

        Console.Write("Connecting... ");
        connection.Start();
        Console.WriteLine("done. Hit: ");
        Console.WriteLine("1:\tSend hello message");
        Console.WriteLine("2:\tRequest => Reply with dynamic reply");
        Console.WriteLine("3:\tRequest => Reply with value type");
        Console.WriteLine("Esc:\tExit");
        Console.WriteLine("");

        var _exit = false;
        while (!_exit)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    Console.Write("Sending hi... ");
                    proxy.Invoke("Ping", Environment.UserName).Finished += (sender, e) =>
                    {
                        Console.WriteLine("done");
                    };
                    break;
                case ConsoleKey.D2:
                    Console.Write("Sending request... ");
                    proxy.Invoke("RequestReplyDynamic").Finished += (sender, e) =>
                    {
                        var _first = e.Result as JToken;
                        Console.WriteLine(" got reply [{0}]", _first["time"].ToString());
                    };
                    break;
                case ConsoleKey.D3:
                    Console.Write("Sending request... ");
                    proxy.Invoke("RequestReplyValueType").Finished += (sender, e) =>
                    {
                        Console.WriteLine("got reply  [{0}]", e.Result);
                    };
                    break;
                case ConsoleKey.Escape:
                    _exit = true;
                    break;
            }
        }
        */
    }
}
 