using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HeartbeatService.Contract
{
    [DataContract]
    public class Heartbeat
    {
        [DataMember]
        public string SenderId { get; private set; }
        [DataMember]
        public DateTime Timestamp { get; private set; }
        [DataMember]
        public string Status { get; private set; }
        public Heartbeat(){}
        public Heartbeat(string senderId, DateTime timestamp, string status)
        {
            this.SenderId = senderId;
            this.Timestamp = timestamp;
            this.Status = status;
        }
    }
}
