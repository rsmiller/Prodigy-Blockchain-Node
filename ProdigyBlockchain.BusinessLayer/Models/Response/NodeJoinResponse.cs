using Prodigy.BusinessLayer.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Response
{
    public class NodeJoinResponse
    {
        public int difficulty { get; set; }
        public bool accepted { get; set; }
        public List<NodeRegisteryDto> nodes { get; set; } = new List<NodeRegisteryDto>();
    }
}
