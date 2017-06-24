using System;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Set connection
            var connection = new HubConnection("http://127.0.0.1:8088/");
            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("GeneralHub");
            //Start connection

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
}


