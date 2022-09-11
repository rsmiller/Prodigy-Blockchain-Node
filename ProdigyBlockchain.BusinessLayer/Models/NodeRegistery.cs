using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models
{
    public class NodeRegistery
    {
        public Guid node_id { get; set; }
        public IPAddress ip_address { get; set; }
        public DateTimeOffset first_seen { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset last_ack { get; set; } = DateTimeOffset.UtcNow;
    }
}
