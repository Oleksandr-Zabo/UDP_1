using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace UDP_Server;

class UdpServer
{
    static Dictionary<string, int> prices = new Dictionary<string, int>
    {
        { "cpu", 250 },
        { "gpu", 500 },
        { "ram", 100 },
        { "ssd", 150 }
    };

    static void Main()
    {
        UdpClient server = new UdpClient(12345);
        Console.WriteLine("UDP server started. Waiting for requests...");

        while (true)
        {
            IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] requestBytes = server.Receive(ref clientEndpoint);

            using (MemoryStream ms = new MemoryStream(requestBytes))
            using (StreamReader reader = new StreamReader(ms))
            {
                string request = reader.ReadToEnd().Trim().ToLower();
                string response = prices.ContainsKey(request) ? $"Price of {request}: {prices[request]}" : "Component not found";

                using (MemoryStream responseStream = new MemoryStream())
                using (StreamWriter writer = new StreamWriter(responseStream))
                {
                    writer.Write(response);
                    writer.Flush();

                    byte[] responseBytes = responseStream.ToArray();
                    server.Send(responseBytes, responseBytes.Length, clientEndpoint);
                    Console.WriteLine($"Request from {clientEndpoint}: {request} -> {response}");
                }
            }
        }
    }
}