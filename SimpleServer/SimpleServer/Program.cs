using System;

namespace SimpleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleServer server = new SimpleServer("127.0.0.1", 4444);

            server.Start();
            server.Stop();
        }
    }
}
