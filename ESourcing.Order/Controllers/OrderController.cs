using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands.OrderCreate;
using Ordering.Application.Queries;
using Ordering.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ESourcing.Order.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<OrderController> logger;

        public OrderController(IMediator mediator, ILogger<OrderController> logger)
        {
            this.mediator = mediator;
            this.logger = logger;
        }

        [HttpGet("GetOrdersBySellerUserName/{userName}")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersBySellerUserName(string userName)
        {
            var query = new GetOrdersBySellerUserNameQuery(userName);

            var orders = await mediator.Send(query);
            if (!orders.Any())
                return NotFound();

            return Ok(orders);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> Create([FromBody]OrderCreateCommand command)
        {
            var result = await mediator.Send(command);
            return Ok(result);
        }

    }
}