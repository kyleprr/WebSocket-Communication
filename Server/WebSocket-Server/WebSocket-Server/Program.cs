using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket_Server;

namespace WebSocketServer
{
    class WebSocketServer
    {
        static void Main(string[] args)
        {
            string serverURL = "http://localhost:1234/";
            WebSocketHandler.StartListening(serverURL).Wait();
        }
    }
}