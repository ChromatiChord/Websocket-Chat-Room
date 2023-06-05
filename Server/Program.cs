class Program
{
    static void Main(string[] args)
    {
        WebSocketServer server = new WebSocketServer();
        server.Start().GetAwaiter().GetResult();
    }
}
