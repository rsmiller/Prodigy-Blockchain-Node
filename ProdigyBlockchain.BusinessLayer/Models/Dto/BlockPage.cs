using Prodigy.BusinessLayer.Blockchain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Dto
{
    public class BlockPage
    {
        public int total_pages { get; set; }
        public int current_page { get; set; }
        public int next_page { get; set; }
        public List<BlockDto> blocks { get; set; } = new List<BlockDto>();
    }
}
