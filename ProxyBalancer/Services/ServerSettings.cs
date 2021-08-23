using System;
using System.Collections.Generic;
using System.Text;

namespace ProxyBalancer.Services
{
    public class ServerSettings
    {
        private static object _lockObject = new object();

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public int InProgressCount { get; private set; }

        public int CompletedCount { get; private set; }

        public void StartService()
        {
            lock(_lockObject)
            {
                InProgressCount++;
            }
        }

        public void FinishService()
        {
            lock(_lockObject)
            {
                InProgressCount--;
                CompletedCount++;
            }
        }
    }
}
