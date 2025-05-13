using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using System.Linq;

namespace UDPServerWPF
{
    public partial class MainWindow : Window
    {
        UdpClient server;
        Dictionary<string, int> prices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) // Case-insensitive matching
        {
            { "CPU", 250 },
            { "GPU", 500 },
            { "RAM", 100 },
            { "SSD", 150 },
            { "Motherboard", 200 },
            { "Power Supply", 120 },
            { "Cooling Fan", 50 }
        };

        Dictionary<string, List<DateTime>> requestLog = new Dictionary<string, List<DateTime>>();
        Dictionary<string, DateTime> lastActive = new Dictionary<string, DateTime>();
        const int REQUEST_LIMIT = 10;
        const int INACTIVITY_TIMEOUT = 10;

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            foreach (var product in prices.Keys)
            {
                ProductList.Items.Add(product);
            }
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            Thread serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.IsBackground = true;
            serverThread.Start();
            LogMessage("Server started!");
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            server?.Close();
            LogMessage("Server stopped!");
        }

        private void StartServer()
        {
            server = new UdpClient(12345);
            while (true)
            {
                IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] requestBytes = server.Receive(ref clientEndpoint);
                string clientIP = clientEndpoint.Address.ToString();

                string request = Encoding.UTF8.GetString(requestBytes).Trim();
                Dispatcher.Invoke(() =>
                {
                    ClientList.Items.Add($"Request from {clientIP}: {request}");
                    LogMessage($"Client {clientIP} requested: {request} at {DateTime.UtcNow}");
                });

                string response = prices.TryGetValue(request, out int price)
                    ? $"Price of {request}: ${price}"
                    : "Component not found";

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                server.Send(responseBytes, responseBytes.Length, clientEndpoint);
            }
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() => LogBox.Text += message + Environment.NewLine);
        }
    }
}
