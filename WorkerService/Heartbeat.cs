using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService
{
    public class Heartbeat
    {
        public string senderId { get;}
        public DateTime timestamp { get; }
        public string status { get; }

        public Heartbeat(string senderId, DateTime timestamp, string status)
        {
            this.senderId = senderId;
            this.timestamp = timestamp;
            this.status = status;
        }
    }
}
