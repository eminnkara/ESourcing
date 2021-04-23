using Esourcing.Sourcing.Entities;
using Esourcing.Sourcing.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ESourcing.Sourcing.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IBidRepository bidRepository;
        private readonly ILogger<BidController> logger;

        public BidController(IBidRepository bidRepository, ILogger<BidController> logger)
        {
            this.bidRepository = bidRepository;
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> SendBid([FromBody] Bid bid)
        {
            await bidRepository.SendBid(bid);

            return Ok();
        }

        [HttpGet("GetBidByAuctionId")]
        [ProducesResponseType(typeof(IEnumerable<Bid>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Bid>>> GetBidByAuctionId(string id)
        {
            IEnumerable<Bid> bids = await bidRepository.GetBidsByAuctionId(id);

            return Ok(bids);
        }

        [HttpGet("GetAllBidsByAuctionId")]
        [ProducesResponseType(typeof(List<Bid>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Bid>>> GetAllBidsByAuctionId(string id)
        {
            IEnumerable<Bid> bids = await bidRepository.GetAllBidsByAuctionId(id);

            return Ok(bids);
        }

        [HttpGet("GetWinnerBid")]
        [ProducesResponseType(typeof(Bid), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Bid>> GetWinnerBid(string id)
        {
            Bid bid = await bidRepository.GetWinnerBid(id);

            return Ok(bid);
        }
    }
}