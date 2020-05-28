using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService
{
    public class Heartbeat
    {
        private string senderId;
        private DateTime timestamp;
        private string status;

        public Heartbeat(string senderId, DateTime timestamp, string status)
        {
            this.senderId = senderId;
            this.timestamp = timestamp;
            this.status = status;
        }
    }
}
