using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace UDP_1
{
    class UDP_Server
    {
        static Dictionary<string, int> prices = new Dictionary<string, int>
        {
            { "cpu", 250 },
            { "gpu", 500 },
            { "ram", 100 },
            { "ssd", 150 }
        };

        static Dictionary<string, List<DateTime>> requestLog = new Dictionary<string, List<DateTime>>();
        static Dictionary<string, DateTime> lastActive = new Dictionary<string, DateTime>();
        const int REQUEST_LIMIT = 10; // Max requests per hour per client
        const int MAX_CLIENTS = 5; // Maximum number of concurrent clients
        const int INACTIVITY_TIMEOUT = 1; // Inactivity timeout in minutes

        static void Main()
        {
            UdpClient server = new UdpClient(12345);
            Console.WriteLine("UDP server started. Waiting for requests...");

            while (true)
            {
                IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] requestBytes = server.Receive(ref clientEndpoint);
                string clientIP = clientEndpoint.Address.ToString();

                // Clean up inactive clients
                CleanupInactiveClients();

                // Check the number of connected clients
                if (!lastActive.ContainsKey(clientIP) && lastActive.Count >= MAX_CLIENTS)
                {
                    string limitResponse = "Server is full. Please try again later.";
                    server.Send(Encoding.UTF8.GetBytes(limitResponse), limitResponse.Length, clientEndpoint);
                    Console.WriteLine($"Client {clientIP} denied due to max connections.");
                    continue;
                }

                // Update the last active time of the client
                lastActive[clientIP] = DateTime.UtcNow;

                // Limit requests per client
                if (!requestLog.ContainsKey(clientIP))
                {
                    requestLog[clientIP] = new List<DateTime>();
                }

                requestLog[clientIP].Add(DateTime.UtcNow);
                requestLog[clientIP] = requestLog[clientIP].Where(t => t > DateTime.UtcNow.AddHours(-1)).ToList();

                if (requestLog[clientIP].Count > REQUEST_LIMIT)
                {
                    string limitResponse = "Request limit exceeded (max 10 per hour)";
                    server.Send(Encoding.UTF8.GetBytes(limitResponse), limitResponse.Length, clientEndpoint);
                    Console.WriteLine($"Client {clientIP} exceeded request limit.");
                    continue;
                }

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
                        Console.WriteLine($"Request from {clientIP}: {request} -> {response}");
                    }
                }
            }
        }

        static void CleanupInactiveClients()
        {
            DateTime now = DateTime.UtcNow;
            List<string> inactiveClients = lastActive
                .Where(kvp => (now - kvp.Value).TotalMinutes > INACTIVITY_TIMEOUT)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var clientIP in inactiveClients)
            {
                lastActive.Remove(clientIP);
                requestLog.Remove(clientIP);
                Console.WriteLine($"Client {clientIP} removed due to inactivity.");
            }
        }
    }
}
