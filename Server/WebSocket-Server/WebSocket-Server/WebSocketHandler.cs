using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WebSocket_Server
{
    internal class WebSocketHandler
    {

        // Asynchronous task that starts the WebSocket server //
        public static async Task StartListening(string url)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(url);
            httpListener.Start();

            Console.WriteLine("WebSocket server started...");

        }




    }
}
