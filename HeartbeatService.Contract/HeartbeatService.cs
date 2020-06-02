using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartbeatService.Contract
{
    public class Heartbeat
    {
        public string SenderId { get; }
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
