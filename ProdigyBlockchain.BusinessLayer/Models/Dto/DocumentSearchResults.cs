using Prodigy.BusinessLayer.Models;
using System.Collections.Generic;

namespace Prodigy.BusinessLayer.Models.Dto
{
    public class DocumentSearchResults
    {
        public int returned_results { get; set; } = 0;
        public int total_company_results { get; set; } = 0;
        public List<BlockDto> blocks { get; set; } = new List<BlockDto>();
    }
}
