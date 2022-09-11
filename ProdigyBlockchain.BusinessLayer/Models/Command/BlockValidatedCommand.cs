using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class BlockValidatedCommand
    {
        public Guid node_id { get; set; }
        public Guid block_id { get; set; }
        public int index { get; set; }
        public string previous_hash { get; set; }
        public DateTimeOffset validated_on { get; set; }
    }
}
