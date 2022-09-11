using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Response
{
    public class MinerTransBlockMinedResponse
    {
        public bool validated { get; set; } = false;
        public decimal reward { get; set; }
        public string hash { get; set; }
    }
}
