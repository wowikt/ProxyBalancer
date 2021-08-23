using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyBalancer.Services
{
    public class BalancerService
    {
        public async Task Run()
        {
            const string inputIpAddress = "127.0.0.1";
            var ipAddress = IPAddress.Parse(inputIpAddress);
            var listener = new TcpListener(ipAddress, 6483);
            listener.Start();
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                // каким то образом выбираем, к какому серверу подключаться, получаем из списка его имя и порт

                var task = Task.Run(async () =>
                {
                    const int bufferSize = 330;
                    //string host = "127.0.0.1";
                    //int port = 6482;
                    var serverSettings = ServerSelectorService.GetServer();
                    if (serverSettings == null)
                    {
                        return;
                    }

                    using (var server = new TcpClient(serverSettings.IpAddress, serverSettings.Port))
                    {
                        try
                        {
                            serverSettings.StartService();
                            var clientStream = client.GetStream();
                            var serverStream = server.GetStream();

                            // Почему-то этот простой путь у меня не захотел работать.
                            // Пришлось изобретать обходные пути
                            //await clientStream.CopyToAsync(serverStream);
                            //await serverStream.CopyToAsync(clientStream);
                            var buffer = new byte[bufferSize];
                            int byteCount;
                            while (true)
                            {
                                byteCount = await clientStream.ReadAsync(buffer, 0, bufferSize);

                                Console.WriteLine($"Got request: {byteCount} bytes");
                                await serverStream.WriteAsync(buffer, 0, byteCount);

                                if (byteCount < bufferSize)
                                {
                                    break;
                                }
                            }

                            Console.WriteLine($"Request sent to server {serverSettings.IpAddress}:{serverSettings.Port}");

                            while (true)
                            {
                                byteCount = await serverStream.ReadAsync(buffer, 0, bufferSize);

                                Console.WriteLine($"Got reply: {byteCount} bytes");
                                await clientStream.WriteAsync(buffer, 0, byteCount);
                                await Task.Delay(100);

                                if (!serverStream.DataAvailable)
                                {
                                    break;
                                }
                            }

                            Console.WriteLine($"Reply sent to client from server {serverSettings.IpAddress}:{serverSettings.Port}");
                        }
                        finally
                        {
                            serverSettings.FinishService();
                            client.Dispose();
                        }
                    }
                });
            }
        }
    }
}
