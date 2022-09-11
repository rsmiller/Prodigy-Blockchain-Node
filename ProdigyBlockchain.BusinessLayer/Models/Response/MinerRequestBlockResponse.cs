using Prodigy.BusinessLayer.Models.Dto;
using System;


namespace Prodigy.BusinessLayer.Models.Response
{
    public class MinerRequestBlockResponse
    {
        public BlockDto block { get; set; }
        public Guid node_id { get; set; }
        public bool has_block { get { return this.block != null; } }
        public DateTimeOffset response_time { get; set; }
    }
}
