using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class NodeJoinCommand
    {
        public Guid node_id {  get; set; }
        public string ip_address { get; set; }
        public int difficulty { get; set; }
    }
}
