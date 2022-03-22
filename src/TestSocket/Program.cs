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
            socket.Bind(new IPEndPoint(IPAddress.Any, 9090));
            socket.Listen(10);
            Console.WriteLine("Listening");

            while (true)
            {
                var handler = socket.Accept();
                var body = $"<body><h1>${handler.RemoteEndPoint}</h1></body>";

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