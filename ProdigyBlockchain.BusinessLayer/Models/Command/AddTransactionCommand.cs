using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class AddTransactionCommand
    {
        public string from { get; set; }
        public string to { get; set; }
        public decimal amount { get; set; }
        public Guid document_block_id { get; set; }
        public long created_on { get; set; }
    }
}
