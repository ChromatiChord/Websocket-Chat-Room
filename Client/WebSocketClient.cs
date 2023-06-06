using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

public class WebSocketClient
{
    private ClientWebSocket _client = new ClientWebSocket();

    private Encryption _encryption = new Encryption();

    private string _friendPublicKey = "";
    private string _publicKey;
    private string _privateKey;

    public WebSocketClient(string publicKey, string privateKey)
    {
        _publicKey = publicKey;
        _privateKey = privateKey;
    }

    public async Task ConnectAsync(string address, string base64Credentials)
    {
        try
        {
            _client.Options.SetRequestHeader("Authorization", "Basic " + base64Credentials);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw ex;
        }

        try
        {
            await _client.ConnectAsync(new Uri(address), CancellationToken.None);
        }
        catch (WebSocketException e)
        {
            if (e.InnerException is WebException webException && webException.Response is HttpWebResponse httpWebResponse)
            {
                Console.WriteLine($"Connection failed: {httpWebResponse.StatusCode} ({httpWebResponse.StatusDescription})");
                return;
            }
            else
            {
                throw;
            }
        }
    }

    public async Task CloseAsync()
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
    }

    public async Task SendMessageAsync(string message, string username)
    {
        string finalSendMessage = $"{username}: {message}";

        var sendBuffer = Encoding.UTF8.GetBytes(finalSendMessage);
        await _client.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        ClearCurrentConsoleLine();
    }

    public async Task ReceiveMessagesAsync(string username)
    {
        var receiveBuffer = new byte[1024];
        while (true)
        {
            var result = await _client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                break;
            }

            string encryptedRecievedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
            
            // if (_friendPublicKey == "") {
            //     _friendPublicKey = publicKey;
            // }

            ClearCurrentConsoleLine();
            Console.WriteLine(encryptedRecievedMessage);

            Console.Write($"{username}:111: ");
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
