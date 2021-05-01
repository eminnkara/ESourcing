using AutoMapper;
using Esourcing.Sourcing.Entities;
using Esourcing.Sourcing.Repositories.Interfaces;
using EventBusRabbitMQ.Core;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Producer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ESourcing.Sourcing.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionRepository auctionRepository;
        private readonly IBidRepository bidRepository;
        private readonly IMapper mapper;
        private readonly EventBusRabbitMQProducer eventBus;
        private readonly ILogger<AuctionController> logger;

        public AuctionController(IAuctionRepository auctionRepository, IBidRepository bidRepository, IMapper mapper, EventBusRabbitMQProducer eventBus, ILogger<AuctionController> logger)
        {
            this.auctionRepository = auctionRepository;
            this.bidRepository = bidRepository;
            this.mapper = mapper;
            this.eventBus = eventBus;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Auction>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Auction>>> GetAuctions()
        {
            var auctions = await auctionRepository.GetAuctions();

            return Ok(auctions);
        }

        [HttpGet("{id:length(24)}", Name = "GetAuction")]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<Auction>> GetAuction(string id)
        {
            var auction = await auctionRepository.GetAuction(id);

            if (auction is null)
            {
                logger.LogError($"Auction with id: {id}, hasn't been found in database.");
                return NotFound();
            }

            return Ok(auction);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> UpdateAuction([FromBody] Auction auction)
        {
            return Ok(await auctionRepository.Update(auction));
        }


        [HttpDelete("{id:length(24)}")]
        [ProducesResponseType(typeof(Auction), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Auction>> DeleteAuctionById(string id)
        {
            return Ok(await auctionRepository.Delete(id));
        }

        [HttpPost("CompleteAuction")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        public async Task<ActionResult> CompleteAuction(string id)
        {
            Auction auction = await auctionRepository.GetAuction(id);
            if (auction is null)
                return NotFound();

            if (auction.Status != (int)Status.Active)
            {
                logger.LogError("Auction can not be completed.");
                return BadRequest();
            }

            Bid winnerBid = await bidRepository.GetWinnerBid(id);
            if (winnerBid is null)
                return NotFound();

            OrderCreateEvent eventMessage = mapper.Map<OrderCreateEvent>(winnerBid);
            eventMessage.Quantity = auction.Quantity;

            auction.Status = (int)Status.Closed;
            bool updateResponse = await auctionRepository.Update(auction);
            if (!updateResponse)
            {
                logger.LogError("Auction can not be updated.");
                return BadRequest();
            }

            try
            {
                eventBus.Publish(EventBusConstants.OrderCreateQueue, eventMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR Publishing integration event: {EventId} from {AppName}", eventMessage.Id, "Sourcing");
                throw;
            }
            
            return Accepted();
        }


        [HttpPost("TestEvent")]
        public ActionResult<OrderCreateEvent> TestEvent()
        {
            OrderCreateEvent eventMessage = new OrderCreateEvent();
            eventMessage.AuctionId = "dummy1";
            eventMessage.ProductId = "dummy_product_1";
            eventMessage.Price = 10;
            eventMessage.Quantity = 100;
            eventMessage.SellerUserName = "test@test.com";

            try
            {
                eventBus.Publish(EventBusConstants.OrderCreateQueue, eventMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERROR Publishing integration event: {EventId} from {AppName}", eventMessage.Id, "Sourcing");
                throw;
            }

            return Accepted(eventMessage);
        }
    }
}