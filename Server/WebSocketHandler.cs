using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;

public class WebSocketHandler
{
    private ConcurrentBag<WebSocketData> _sockets = new ConcurrentBag<WebSocketData>();

    public async Task HandleWebSocket(HttpListenerContext context, string username, string roomID)
    {
        var webSocketContext = await context.AcceptWebSocketAsync(null);
        var webSocket = webSocketContext.WebSocket;

        WebSocketData webSocketData = new WebSocketData
        {
            WebSocket = webSocket,
            RoomID = roomID
        };
        
        if (IsRoomFull(roomID)) {
            await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Room is full", CancellationToken.None);
            return; 
        }

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
                Console.WriteLine("-------------------------------------");

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

    private bool IsRoomFull(string roomID) {
        // Count the number of WebSocketData objects with the same roomID
        return _sockets.Count(socket => socket.RoomID == roomID) >= 2;
    }
}
