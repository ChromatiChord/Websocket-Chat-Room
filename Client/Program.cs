class Program
{
    const string WEBSOCKET_ADDRESS = "ws://localhost:8080/";

    static async Task Main(string[] args)
    {
        WebSocketClient client = new WebSocketClient();

        // INITIAL USER INPUT
        Console.Write("Enter your username: ");
        string username = Console.ReadLine() ?? "USER";

        Console.Write("Would you like to Join or Create a Room? (join/create): ");
        string creationPath = Console.ReadLine() ?? "join";

        string roomID = "";

        if (creationPath == "create") {
                Console.Write("Enter your RoomID: ");
                roomID = Console.ReadLine() ?? "123";

                string creationCredentials = ClientHelpers.Base64Encode(String.Format("{0}:{1}", "/////////CREATE_NEW_ROOM//////////", ClientHelpers.GetHash(roomID)));
                await client.ConnectAsync(WEBSOCKET_ADDRESS, creationCredentials);
                await client.CloseAsync();
        } else {
            Console.Write("Enter your RoomID: ");
            roomID = Console.ReadLine() ?? "123";
        }

        string hashedRoomID = ClientHelpers.GetHash(roomID);

        string base64Credentials = ClientHelpers.Base64Encode(String.Format("{0}:{1}", username, hashedRoomID));

        await client.ConnectAsync(WEBSOCKET_ADDRESS, base64Credentials);

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

        var receiveTask = client.ReceiveMessagesAsync(username);

        while (true)
        {
            Console.Write($"{username}: ");
            string message = Console.ReadLine() ?? "MESSAGE";
            if (message == "exit")
            {
                break;
            }
            await client.SendMessageAsync(message, username);
        }

        await client.CloseAsync();

        await receiveTask;
    }
}
