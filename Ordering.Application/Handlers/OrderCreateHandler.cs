using AutoMapper;
using FluentValidation;
using MediatR;
using Ordering.Application.Commands.OrderCreate;
using Ordering.Application.Responses;
using Ordering.Domain.Entities;
using Ordering.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Handlers
{
    public class OrderCreateHandler : IRequestHandler<OrderCreateCommand, OrderResponse>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IMapper mapper;

        public OrderCreateHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            this.orderRepository = orderRepository;
            this.mapper = mapper;
        }

        public async Task<OrderResponse> Handle(OrderCreateCommand request, CancellationToken cancellationToken)
        {
            var orderEntity = mapper.Map<Order>(request);

            if (orderEntity is null)
                throw new ApplicationException("Entity could not be mapped!");

            var order = await orderRepository.AddAsync(orderEntity);
            var orderResponse = mapper.Map<OrderResponse>(order);

            return orderResponse;
        }
    }
}