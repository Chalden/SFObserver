using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService
{
    public class Heartbeat
    {
        public string SenderId { get;}
        public DateTime Timestamp { get; }
        public string Status { get; }

        public Heartbeat(string senderId, DateTime timestamp, string status)
        {
            this.SenderId = senderId;
            this.Timestamp = timestamp;
            this.Status = status;
        }
    }
}
