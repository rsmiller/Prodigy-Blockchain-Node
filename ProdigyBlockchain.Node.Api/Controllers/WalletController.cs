using Prodigy.BusinessLayer;
using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Models.Dto;
using Prodigy.BusinessLayer.Models.Response;
using Prodigy.BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Prodigy.BusinessLayer.Models.Command;

namespace Prodigy.Node.Api.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        public IWalletDataService _DataService;

        public WalletController(IWalletDataService dataService)
        {
            _DataService = dataService;
        }

        [HttpGet("GetWalletTransactions", Name = "GetWalletTransactions")]
        [ProducesResponseType(typeof(PagedResult<List<Transaction>>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult GetWalletTransactions([FromQuery] string wallet_id, int page = 0)
        {
            var result = _DataService.GetWalletTransactions(wallet_id, page);

            return new JsonResult(result);
        }

        [HttpGet("GetWalletStatistics", Name = "GetWalletStatistics")]
        [ProducesResponseType(typeof(Response<WalletStatisticsDto>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult GetWalletStatistics([FromQuery] string wallet_id)
        {
            var result = _DataService.GetWalletStatistics(wallet_id);

            return new JsonResult(result);
        }

        [HttpPost("RequestSpend", Name = "RequestSpend")]
        [ProducesResponseType(typeof(Response<bool>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public ActionResult AddTransaction([FromBody] RequestSpendCommand command)
        {
            var transaction = _DataService.RequestSpend(command);

            return Ok(transaction);
        }

        [HttpPost("CreateWallet", Name = "CreateWallet")]
        [ProducesResponseType(typeof(Response<WalletCreatedDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult CreateWallet([FromBody] CreateWalletCommand command)
        {
            var result = _DataService.CreateWallet(command);

            return Ok(result);
        }

        [HttpPost("GetWallet", Name = "GetWallet")]
        [ProducesResponseType(typeof(Response<WalletStatisticsDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult GetWallet([FromBody] GetWalletCommand command)
        {
            var result = _DataService.LoadWallet(command);

            return Ok(result);
        }
    }
}
