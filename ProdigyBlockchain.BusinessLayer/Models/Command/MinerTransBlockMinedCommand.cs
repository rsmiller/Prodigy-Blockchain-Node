using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class MinerTransBlockMinedCommand
    {
        [Required]
        public MiningUser miner { get; set; }
        [Required]
        public Guid block_id { get; set; }
        [Required]
        public string block_hash { get; set; }
    }
}
