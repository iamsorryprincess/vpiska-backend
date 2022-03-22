using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestSocket
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090));
            socket.Listen(10);
            Console.WriteLine("Listening");

            while (true)
            {
                var handler = socket.Accept();
                Console.WriteLine($"{socket.LocalEndPoint}");
                Console.WriteLine($"{socket.RemoteEndPoint}");
                Console.WriteLine($"{handler.LocalEndPoint}");
                Console.WriteLine($"{handler.RemoteEndPoint}");
                var body = $"<body><h1>{handler.RemoteEndPoint}</h1></body>";

                var response = @$"HTTP/1.1 200 Ok
Server: test
Content-Type: text/html
Connection: close
Content-Length: {body.Length}

{body}";
                
                handler.Send(Encoding.UTF8.GetBytes(response));
                handler.Close();
            }
        }
    }
}