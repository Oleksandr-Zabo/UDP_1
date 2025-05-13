using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Threading;
using System.Collections.Generic;

namespace UDPClientWPF
{
    public partial class MainWindow : Window
    {
        UdpClient client;
        IPEndPoint serverEndpoint;
        List<string> products = new List<string>
        {
            "CPU",
            "GPU",
            "RAM",
            "SSD",
            "Motherboard",
            "Power Supply",
            "Cooling Fan"
        };

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            foreach (var product in products)
            {
                ProductList.Items.Add(product);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            client = new UdpClient();
            serverEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
            MessageBox.Show("Connected to server!");
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            if (ProductList.SelectedItem == null)
            {
                MessageBox.Show("Please select a product.");
                return;
            }

            string request = ProductList.SelectedItem.ToString();
            byte[] requestBytes = Encoding.UTF8.GetBytes(request);
            client.Send(requestBytes, requestBytes.Length, serverEndpoint);

            Thread receiveThread = new Thread(() =>
            {
                byte[] responseBytes = client.Receive(ref serverEndpoint);
                string response = Encoding.UTF8.GetString(responseBytes);
                Dispatcher.Invoke(() => ResponseList.Items.Add($"Server: {response}"));
            });

            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
    }
}