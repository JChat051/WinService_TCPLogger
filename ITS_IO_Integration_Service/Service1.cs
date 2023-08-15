using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Timers;
using System.IO;
using System.Net;
using System.Threading;

namespace ITS_IO_Integration_Service
{
    public partial class Service1 : ServiceBase
    {
        private TcpListener tcpListener;
        private CancellationTokenSource cancellationTokenSource;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            cancellationTokenSource = new CancellationTokenSource();
            StartListening();
        }
        protected override void OnStop()
        {
            cancellationTokenSource.Cancel();
            tcpListener.Stop();
        }

        private async void StartListening()
        {
            IPAddress ipAddress = IPAddress.Any;
            int port = 13010;

            tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Start();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    LogReceivedData(ex.Message+"Encountered after cancellationtokensource");
                }
            }
        }


        private async Task HandleClientAsync(TcpClient client)
        {
            using (NetworkStream networkStream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                StringBuilder receivedData = new StringBuilder();

                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    receivedData.Append(data);

                    // Log the received data
                    LogReceivedData(data);
                }

                client.Close();
            }
        }

        private void LogReceivedData(string data)
        {
            string logFilePath = "F:\\TcpListenerLogs\\ReceivedDataLog.txt";

            try
            {
                // Ensure the directory exists
                string logDirectory = Path.GetDirectoryName(logFilePath);
                Directory.CreateDirectory(logDirectory);

                // Write received data to the log file
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"Received at: {DateTime.Now}");
                    writer.WriteLine(data);
                    writer.WriteLine(new string('-', 40));
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine("Encountered in logging function");
                    writer.WriteLine(ex.Message);
                    
                }
            }
        }



    }
}
