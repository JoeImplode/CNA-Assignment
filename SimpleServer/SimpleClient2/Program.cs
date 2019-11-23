using System;

namespace SimpleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleClient client = new SimpleClient();
            if (client.Connect("127.0.0.1", 4444) == true)
            {
                client.Run();
            }
            else
            {
                Console.WriteLine("Failed");
            }
        }
    }
}
