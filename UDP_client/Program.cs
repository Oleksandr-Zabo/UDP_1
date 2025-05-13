namespace UDP_client;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDP_client
{
    static void Main()
    {
        UdpClient client = new UdpClient();
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);

        while (true)
        {
            Console.Write("Enter component name (or type 'exit' to disconnect): ");
            string? request = Console.ReadLine();

            if (request?.ToLower() == "exit")
            {
                Console.WriteLine("Disconnecting...");
                break;
            }

            if (request != null)
            {
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                client.Send(requestBytes, requestBytes.Length, serverEndpoint);
            }

            byte[] responseBytes = client.Receive(ref serverEndpoint);
            string response = Encoding.UTF8.GetString(responseBytes);

            Console.WriteLine($"Server response: {response}");
        }

        client.Close();
    }
}
