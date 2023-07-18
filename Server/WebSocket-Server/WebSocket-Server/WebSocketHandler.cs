using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocket_Server
{
    internal class WebSocketHandler
    {
        private const int BufferSize = 1024;
        private static readonly ConcurrentDictionary<string, WebSocket> Clients = new ConcurrentDictionary<string, WebSocket>();

        private enum CommandType
        {
            Unknown,
            A,
            B,
            C
        }

        // Asynchronous task that starts the WebSocket server //
        public static async Task StartListening(string url)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(url);
            httpListener.Start();

            Console.WriteLine("WebSocket Server Started...");

            while (true) // Enter an infinite loop to accept incoming HTTP contexts
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest) // Checks if the request is a WebSocket request
                {
                    ProcessWebSocketRequest(context);
                }
                else // If it is not a WebSocket request, set the response status code to 400 (Bad Request) and closes the response
                {
                    context.Response.StatusCode = 400;
                    string message = "{\"statusCode\":400,\"error\":\"Bad Request\",\"message\":\"Invalid WebSocket Request\"}";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    context.Response.ContentLength64 = messageBytes.Length;
                    context.Response.OutputStream.Write(messageBytes, 0, messageBytes.Length);
                    context.Response.OutputStream.Close();
                }
            }
        }


        // Handles the WebSocket handshake and initialises a WebSocket connection //
        private static async void ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                string clientId = Guid.NewGuid().ToString(); // Generate a unique client ID 
                Clients.TryAdd(clientId, webSocket); // Add to client dictionary
                Console.WriteLine($"Client connected: Client #{clientId}");

                await HandleClient(webSocket, clientId); // Handle the WebSocket communication with the client
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket Error: {ex}");
            }
            finally
            {
                if (webSocketContext != null)
                    webSocketContext.WebSocket.Dispose();
            }
        }


        // Handling the communication with an individual WebSocket client //
        private static async Task HandleClient(WebSocket webSocket, string clientId)
        {
            var buffer = new byte[BufferSize];

            try
            {

                while (webSocket.State == WebSocketState.Open) // Enter a loop that runs as long as the WebSocket connection is open
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None); // Receive data from the client

                    if (receiveResult.MessageType == WebSocketMessageType.Close) // Close connection if close message received
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                        WebSocket removedSocket;
                        Clients.TryRemove(clientId, out removedSocket);
                        Console.WriteLine($"Client disconnected: Client #{clientId}");
                    }
                    else // If the received message type is data
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count); // Extract the received message from the buffer
                        Console.WriteLine($"Received from Client #{clientId}: {message}");

                        CommandType commandType = GetCommandType(message); // Get the command type from the message

                        switch (commandType)
                        {
                            case CommandType.A:
                                // Handle Command A
                                SendDataAsync(webSocket, "{\"data\":{\"id\":1,\"firstName\":\"Kyle\",\"lastName\":\"Pereira\"}}");
                                break;
                            case CommandType.B:
                                // Handle Command B
                                SendDataAsync(webSocket, "{\"data\":{\"id\":2,\"firstName\":\"Kyle\",\"lastName\":\"Pereira\"}}");
                                break;
                            case CommandType.C:
                                // Handle Command C
                                SendDataAsync(webSocket, "{\"data\":{\"id\":3,\"firstName\":\"Kyle\",\"lastName\":\"Pereira\"}}");
                                break;
                            case CommandType.Unknown:
                                // Unknown command
                                SendDataAsync(webSocket, "{\"statusCode\":404,\"error\":\"Not Found\",\"message\":\"Invalid Request\",\"request\":\"Return Command\"}");
                                break;
                        }

                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket Error: {ex.Message}");
                WebSocket removedSocket;
                Clients.TryRemove(clientId, out removedSocket);
                Console.WriteLine($"Client disconnected: Client #{clientId}");
            }
        }

        private static CommandType GetCommandType(string message)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(message); // Parse the JSON message

                if (jsonDocument.RootElement.TryGetProperty("request", out var commandProperty)) // Check if the "command" property exists in the JSON
                {
                    string command = commandProperty.GetString();

                    if (!int.TryParse(command, out _)) // Check if the command is not a numerical value
                    {
                        if (Enum.TryParse<CommandType>(command, false, out CommandType commandType)) // Case-sensitive enum parsing
                        {
                            if (commandType.ToString() == command) // Check if the parsed enum value matches the command exactly
                            {
                                return commandType;
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Command error: {ex}");
            }

            return CommandType.Unknown;
        }



        private static async Task<bool> SendDataAsync(WebSocket webSocket, string message)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send data: {ex.Message}");
                return false;
            }
        }



    }
}
