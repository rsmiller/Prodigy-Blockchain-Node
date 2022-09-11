using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Command;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models.Response;
using Prodigy.BusinessLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;


namespace Prodigy.Node.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NodeController : ControllerBase
    {
        public INodeDataService _DataService;

        public NodeController(INodeDataService dataService)
        {
            _DataService = dataService;
        }

        [HttpGet("Ping", Name = "Ping")]
        [ProducesResponseType(typeof(PingResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult Ping()
        {
            var response = _DataService.Ping();

            return Ok(response);
        }

        [HttpPost("RequestToJoinNetwork", Name = "RequestToJoinNetwork")]
        [ProducesResponseType(typeof(NodeJoinResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult RequestToJoinNetwork([FromBody] NodeJoinCommand command)
        {
            var result = _DataService.RequestToJoinNetwork(command);

            return new JsonResult(result);
        }

        [HttpPost("CertCreated", Name = "CertCreated")]
        [ProducesResponseType(typeof(MinerBlockMinedResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult CertCreated([FromBody] DocumentCreateCommand command)
        {
            var result = _DataService.CertCreated(command);

            return new JsonResult(result);
        }
    }
}
