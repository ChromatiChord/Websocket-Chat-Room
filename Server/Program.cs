using System;
using System.ComponentModel;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

class WebSocketData
{
    public WebSocket WebSocket { get; set; }
    public string RoomID { get; set; }
}

class Program
{
    const string WEBSOCKET_ADDRESS = "http://localhost:8080/";
    
    static void Main(string[] args)
    {
        StartWebsocketServer().GetAwaiter().GetResult();
    }

    public static async Task StartWebsocketServer()
    {
        // SETUP
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(WEBSOCKET_ADDRESS);
        listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
        listener.Start();

        Console.WriteLine("Listening...");

        while (true)
        {
            var context = await listener.GetContextAsync();
            
            // ON CONNECTION
            if (context.Request.IsWebSocketRequest)
            {
                var authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null)
                {
                    (string username, string roomID) = ExtractCredentials(authHeader);

                    if (username == "/////////CREATE_NEW_ROOM//////////") {
                        ServerHelpers.WriteToDB(roomID);
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Created new room \'{roomID}\'");
                        Console.ResetColor();
                    } else {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Connected to {username}!");
                        Console.ResetColor();

                        if (IsRoomValid(roomID))
                        {
                            _ = HandleWebSocket(context, username, roomID); 
                        }
                        else
                        {
                            // KICK THEM OUT HERE
                            context.Response.StatusCode = 403; // Forbidden
                            context.Response.StatusDescription = "Invalid room ID";
                            context.Response.Close();
                        }
                    }
                }
            }
        }
}


    private static ConcurrentBag<WebSocketData> _sockets = new ConcurrentBag<WebSocketData>();

    private static async Task HandleWebSocket(HttpListenerContext context, string username, string roomID)
    {
        var webSocketContext = await context.AcceptWebSocketAsync(null);
        var webSocket = webSocketContext.WebSocket;

        WebSocketData webSocketData = new WebSocketData
        {
            WebSocket = webSocket,
            RoomID = roomID
        };

        _sockets.Add(webSocketData);
        byte[] buffer = new byte[1024];

        var encodedMessageToSend = Encoding.UTF8.GetBytes($"{username} has joined the chat!");
        foreach (var socket in _sockets)
        {
            if (socket.WebSocket.State == WebSocketState.Open && socket.RoomID == roomID && socket.WebSocket != webSocket)
            {
                await socket.WebSocket.SendAsync(new ArraySegment<byte>(encodedMessageToSend, 0, encodedMessageToSend.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        while (webSocket.State == WebSocketState.Open)
        {
            var receivedResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receivedResult.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            else
            {
                string receivedString = Encoding.UTF8.GetString(buffer, 0, receivedResult.Count);
                Console.WriteLine(receivedString);

                foreach (var socket in _sockets)
                {
                    if (socket.WebSocket.State == WebSocketState.Open && socket.RoomID == roomID && socket.WebSocket != webSocket)
                    {
                        await socket.WebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, receivedResult.Count), WebSocketMessageType.Text, receivedResult.EndOfMessage, CancellationToken.None);
                    }
                }
            }
        }
        
        Console.WriteLine("Closing Sockets");
        _sockets = new ConcurrentBag<WebSocketData>(_sockets.Where(socket => socket.WebSocket != webSocket));
    }

    private static (string, string) ExtractCredentials(string credentials) 
    {
        string new_creds = ServerHelpers.Base64Decode(credentials.Replace("Basic ", ""));
        string username = new_creds.Split(":")[0];
        string roomID = new_creds.Split(":")[1];

        return (username, roomID);
    }

    private static bool IsRoomValid(string roomID)
    {
        string[] validRooms = ServerHelpers.ReadDBToArray();
        return validRooms.Any(roomID.Contains);
    }

}
