using System;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;

class Program
{
    const string WEBSOCKET_ADDRESS = "ws://localhost:8080/";

    private static async Task Main(string[] args)
    {
        ClientWebSocket client = new ClientWebSocket();

        Console.Write("Enter your username: ");
        string username = Console.ReadLine() ?? "USER";

        Console.Write("Would you like to Join or Create a Room? (join/create): ");
        string creationPath = Console.ReadLine() ?? "join";

        string roomID = "";

        if (creationPath == "create") {
                Console.Write("Enter your RoomID: ");
                roomID = Console.ReadLine() ?? "123";

                string creationCredentials = ClientHelpers.Base64Encode(String.Format("{0}:{1}", "/////////CREATE_NEW_ROOM//////////", ClientHelpers.GetHash(roomID)));
                client.Options.SetRequestHeader("Authorization", "Basic " + creationCredentials);
                await client.ConnectAsync(new Uri(WEBSOCKET_ADDRESS), CancellationToken.None);
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);

        } else {
            Console.Write("Enter your RoomID: ");
            roomID = Console.ReadLine() ?? "123";
        }

        string hashedRoomID = ClientHelpers.GetHash(roomID);

        string base64Credentials = ClientHelpers.Base64Encode(String.Format("{0}:{1}", username, hashedRoomID));

        try
        {
            // Add authentication header with the username and roomID
            client.Options.SetRequestHeader("Authorization", "Basic " + base64Credentials);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw ex;
        }

        // Connect to the WebSocket server
        try
        {
            // Connect to the WebSocket server
            await client.ConnectAsync(new Uri(WEBSOCKET_ADDRESS), CancellationToken.None);
        }
        catch (WebSocketException e)
        {
            // Check if it is an HTTP exception
            if (e.InnerException is WebException webException && webException.Response is HttpWebResponse httpWebResponse)
            {
                // Print the status code and description
                Console.WriteLine($"Connection failed: {httpWebResponse.StatusCode} ({httpWebResponse.StatusDescription})");
                return;
            }
            else
            {
                throw;
            }
        }

        Console.Clear();
        Console.WriteLine("------------------------------------------------------------");
        Console.BackgroundColor = ConsoleColor.Red;
        Console.WriteLine("Connected!");
        Console.ResetColor();
        Console.WriteLine("Enter a message to send to the server, or 'exit' to exit.");
        Console.Write($"Connected to Room \'");
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write($"{roomID}");
        Console.ResetColor();
        Console.WriteLine("\'.");
        Console.WriteLine("------------------------------------------------------------");

        // Start a separate task to receive messages from the server
        var receiveTask = ReceiveMessages(client, username);

        while (true)
        {
            Console.Write($"{username}: ");
            string message = Console.ReadLine() ?? "MESSAGE";
            if (message == "exit")
            {
                break;
            }
            await SendMessage(client, message, username);
        }

        // Close the connection
        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);

        // Wait for the receive task to complete before exiting
        await receiveTask;
    }

    private static async Task SendMessage(ClientWebSocket client, string message, string username)
    {
        // Send a message to the WebSocket server
        var sendBuffer = Encoding.UTF8.GetBytes($"{username}: {message}");
        await client.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        ClearCurrentConsoleLine();
    }

    private static async Task ReceiveMessages(ClientWebSocket client, string username)
    {
        var receiveBuffer = new byte[1024];
        while (true)
        {
            var result = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

            // Check if the WebSocket connection has been closed
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                break;
            }

            // Print the received message
            var receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
            ClearCurrentConsoleLine();
            Console.WriteLine(receivedMessage);
            Console.Write($"{username}: ");
        }
    }

    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, currentLineCursor);
    }
}
