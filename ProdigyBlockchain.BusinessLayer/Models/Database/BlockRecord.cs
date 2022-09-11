using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Database
{
    public class BlockRecord
    {
        
        public int id { get; set; }
        public int index { get; set; }
        public Guid block_id { get; set; }
        public int transaction_count { get; set; }
        public long block_size { get; set; }
        public decimal sum_of_payments { get; set; }
        public DateTime mined_on { get; set; }
        public DateTime created_on { get; set; }
    }
}
