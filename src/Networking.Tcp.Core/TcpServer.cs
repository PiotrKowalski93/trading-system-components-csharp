using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Networking.Tcp.Core
{
    public class TcpServer
    {
        readonly int _port;
        readonly TcpListener _listener;

        //private List<TcpClient> _tcpClients;

        public TcpServer(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);

            //_tcpClients = new List<TcpClient>();
        }

        public void Listen()
        {
            Console.WriteLine($"Starting TCP Server on port {_port}");
            _listener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                var client = _listener.AcceptTcpClient();
                //_tcpClients.Add(client);

                Console.WriteLine("Client connected!");

                var stream = client.GetStream();
                var buffer = new byte[1024];
                int bytesRead;

                // Read and echo data
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {message}");

                    // Echo back the message
                    stream.Write(buffer, 0, bytesRead);
                    Console.WriteLine("Message echoed back.");
                }

                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

    }
}
