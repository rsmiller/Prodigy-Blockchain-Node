using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.Models.Dto
{
    public class NodeListDto
    {
        public int difficulty {  get; set; }
        public List<string> nodes { get; set; } = new List<string>();
    }
}
