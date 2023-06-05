using System;
using System.Net;

public class Authentication
{
    private WebSocketHandler _handler = new WebSocketHandler();

    public bool AuthenticateAndHandle(HttpListenerContext context, string authHeader)
    {
        (string username, string roomID) = ExtractCredentials(authHeader);

        if (username == "/////////CREATE_NEW_ROOM//////////") 
        {
            ServerHelpers.WriteToDB(roomID);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"Created new room \'{roomID}\'");
            Console.ResetColor();
            return true;
        } 
        else 
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"Connected to {username}!");
            Console.ResetColor();

            if (IsRoomValid(roomID))
            {
                _ = _handler.HandleWebSocket(context, username, roomID);
                return true;
            }
            else
            {
                return false;
            }
        }
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
