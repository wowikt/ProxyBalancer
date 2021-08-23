using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyBalancer.Services
{
    public static class ServerSelectorService
    {
        private static List<ServerSettings> _serverSettings = new List<ServerSettings>();
        private static object _lockObject = new object();
        private static int _lastServerIndex = -1;

        static ServerSelectorService()
        {
            for (int portNum = 4830; portNum <= 4834; portNum++)
            {
                _serverSettings.Add(new ServerSettings
                {
                    IpAddress = "127.0.0.1",
                    Port = portNum,
                });
            }
        }

        public static ServerSettings GetServer()
        {
            if (!_serverSettings.Any())
            {
                _lastServerIndex = -1;
                return null;
            }

            lock (_lockObject)
            {
                var result = _serverSettings
                    // "Индекс" определяется исходя из индекса последнего выбранного сервиса
                    .Select((s, i) => new { Server = s, Index = i > _lastServerIndex ? i : i + _serverSettings.Count })
                    // Упорядочиваем по:
                    // 1. Текущему обрабатываемому количеству запросов
                    .OrderBy(s => s.Server.InProgressCount)
                    // 2. Ранее обработанному количеству запросов
                    .ThenBy(s => s.Server.CompletedCount)
                    // 3. По индексу (в первую очередь берутся те, которые следуют сразу после предыдущего выбранного сервиса)
                    .ThenBy(s => s.Index)
                    .First();
                _lastServerIndex = result.Index;
                return result.Server;
            }
        }
    }
}
