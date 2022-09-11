using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer;
using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Command;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models.Response;
using Prodigy.BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prodigy.Node.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BlockchainController : ControllerBase
    {
        public IBlockchainDataService _DataService;

        public BlockchainController(IBlockchainDataService dataService)
        {
            _DataService = dataService;
        }

        [HttpGet("SearchDocument", Name = "SearchDocument")]
        [ProducesResponseType(typeof(DocumentSearchResults), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult SearchCerts([FromQuery] Guid? customer_id, string wildcard)
        {
            var result = _DataService.SearchDocument(customer_id, wildcard);

            return new JsonResult(result);
        }

        [HttpGet("SearchByCustomer", Name = "SearchByCustomer")]
        [ProducesResponseType(typeof(DocumentSearchResults), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult SearchByCustomer([FromQuery] Guid customer_id, string wildcard, int page = 0)
        {
            var result = _DataService.SearchByCustomer(customer_id, wildcard, page);

            return new JsonResult(result);
        }

        [HttpPost("RequestBlock", Name = "RequestBlock")]
        [ProducesResponseType(typeof(MinerRequestBlockResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult MinerRequestBlock([FromBody] MiningUser user)
        {
            var result = _DataService.MinerRequestBlock(user);

            return new JsonResult(result);
        }

        [HttpPost("BlockValidated", Name = "BlockValidated")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult BlockValidated([FromBody] BlockValidatedCommand command)
        {
            _DataService.BlockValidated(command);

            return Ok();
        }

        [HttpGet("MinerJoin", Name = "MinerJoin")]
        [ProducesResponseType(typeof(NodeListDto), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult MinerJoin([FromQuery] MinerJoinCommand command)
        {
            var result = _DataService.MinerJoin_Ack(command);

            return new JsonResult(result);
        }

        [HttpPost("MinedBlock", Name = "MinedBlock")]
        [ProducesResponseType(typeof(MinerBlockMinedResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult MinedBlock([FromBody] MinerBlockMinedCommand command)
        {
            var result = _DataService.MinerBlockMined(command);

            return new JsonResult(result);
        }

        [HttpGet("GetLatestBlocks", Name = "GetLatestBlocks")]
        [ProducesResponseType(typeof(Response<List<BlockDto>>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult GetLatestBlocks()
        {
            var result = _DataService.GetLatestBlocks();

            return new JsonResult(result);
        }

        [HttpGet("GetLatestTransactions", Name = "GetLatestTransactions")]
        [ProducesResponseType(typeof(Response<List<Transaction>>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult GetLatestTransactions()
        {
            var result = _DataService.GetLatestTransactions();

            return new JsonResult(result);
        }

        [HttpGet("DownloadBlockchain", Name = "DownloadBlockchain")]
        [ProducesResponseType(typeof(BlockPage), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> DownloadBlockchain([FromQuery] int page)
        {
            var result = await _DataService.DownloadBlockchain(page);

            return new JsonResult(result);
        }

        [HttpGet("DownloadPendingBlockchain", Name = "DownloadPendingBlockchain")]
        [ProducesResponseType(typeof(BlockPage), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult DownloadPendingBlockchain([FromQuery] int page)
        {
            var result = _DataService.DownloadPendingBlockchain(page);

            return new JsonResult(result);
        }

        [HttpPost("BlockAdded", Name = "BlockAdded")]
        [ProducesResponseType(typeof(MinerBlockMinedResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult BlockAdded([FromBody] BlockDto dto)
        {
            _DataService.BlockAdded(dto);

            return Ok();
        }

        [HttpPost("AddTransaction", Name = "AddTransaction")]
        [ProducesResponseType(typeof(MinerBlockMinedResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult AddTransaction([FromBody] AddTransactionCommand command)
        {
            var transaction = _DataService.AddTransaction(command);

            return Ok(transaction);
        }

        [HttpGet("GetBlockById", Name = "GetBlockById")]
        [ProducesResponseType(typeof(Response<BlockDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> GetBlock([FromQuery] Guid block_id)
        {
            var result = await _DataService.GetBlockById(block_id);

            return new JsonResult(result);
        }

        [HttpGet("GetBlocksByIdentifier", Name = "GetBlocksByIdentifier")]
        [ProducesResponseType(typeof(Response<List<BlockDto>>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> GetBlocksByIdentifier([FromQuery] string identifier, string wildcard, bool include_data = false)
        {
            var result = await _DataService.GetBlocksByIdentifier(identifier, wildcard, include_data);

            return new JsonResult(result);
        }

        [HttpGet("GetTransaction", Name = "GetTransaction")]
        [ProducesResponseType(typeof(Response<Transaction>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult GetTransaction([FromQuery] string txn)
        {
            var result = _DataService.GetTransaction(txn);

            return new JsonResult(result);
        }

        [HttpGet("GetDocument", Name = "GetDocument")]
        [ProducesResponseType(typeof(FileStreamResult), 200)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> GetDocument([FromQuery] Guid block_id)
        {
            var result = await _DataService.GetDocument(block_id);

            if (!result.Success)
                return BadRequest(result);

            return File(result.Data, "application/pdf");
        }
    }
}
