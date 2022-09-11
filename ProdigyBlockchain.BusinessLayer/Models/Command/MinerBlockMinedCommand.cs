
using System;
using System.ComponentModel.DataAnnotations;

namespace Prodigy.BusinessLayer.Models.Command
{
    public class MinerBlockMinedCommand
    {
        [Required]
        public MiningUser miner { get; set; }
        [Required]
        public Guid block_id { get; set; }
        [Required]
        public string block_hash { get; set; }
    }
}
