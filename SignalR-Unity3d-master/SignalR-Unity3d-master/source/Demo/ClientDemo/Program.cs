//using Newtonsoft.Json.Linq;
using SignalR.Client._20.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ClientDemo
{
    class Program
    {
        static void Main(string[] args)
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
                Console.WriteLine(string.Format("Received: [{0}] from {1}",
                    _first["message"].ToString(), _first["from"].ToString()));
                    */
            };

            connection.Closed += () => Console.WriteLine("Closed");
            connection.Error += x => Console.WriteLine("Error: " + x.Message);
            connection.Received += x => Console.WriteLine("Recived: " + x);
            connection.Reconnected += () => Console.WriteLine("Reconected");

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
                    case ConsoleKey.F1:
                        Console.Write("Sending hi... ");
                        proxy.Invoke("SendChatMessage", "JS CLient", "I am working").Finished += (sender, e) =>
                        {
                            Console.WriteLine("done");
                        };
                        break;
                    case ConsoleKey.Escape:
                        _exit = true;
                        break;
                }
            }
        }
    }
}
