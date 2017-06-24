using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client;

public class Main : MonoBehaviour
{
	void Start ()
    {
        var connection = new HubConnection("https://quisutdeus.in");
        var myHub = connection.CreateHubProxy("GeneralHub");

        connection.Start().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Console.WriteLine("There was an error opening the connection:{0}",
                                  task.Exception.GetBaseException());
            }
            else
            {
                Console.WriteLine("Connected");
            }

        }).Wait();

        myHub.Invoke<string, string>("SendChatMessage", (x => Console.WriteLine("Progress: " + x)), "JS CLient", "I am working").ContinueWith(task => {
            if (task.IsFaulted)
            {
                Console.WriteLine("There was an error calling send: {0}",
                                  task.Exception.GetBaseException());
            }
            else
            {
                Console.WriteLine(task.Result);
            }
        });

        myHub.On<string, string>("addNewMessageToPage", (param, param2) => {
            Console.WriteLine(param + ": " + param2);
        });

        //myHub.Invoke<string>("DoSomething", "I'm doing something!!!").Wait();

        Console.Read();
        connection.Stop();
    }
}