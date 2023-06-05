using System.Net;

public class WebSocketServer
{
    const string WEBSOCKET_ADDRESS = "http://localhost:8080/";

    private Authentication _auth = new Authentication();

    public async Task Start()
    {
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
                    if(!_auth.AuthenticateAndHandle(context, authHeader))
                    {
                        context.Response.StatusCode = 403; // Forbidden
                        context.Response.StatusDescription = "Invalid room ID";
                        context.Response.Close();
                    }
                }
            }
        }
    }
}
