using AutoMapper;
using MediatR;
using Ordering.Application.Queries;
using Ordering.Application.Responses;
using Ordering.Domain.Entities;
using Ordering.Domain.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Commands.OrderCreate
{
    public class GetOrdersBySellerUserNameHandler : IRequestHandler<GetOrdersBySellerUserNameQuery, IEnumerable<OrderResponse>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IMapper mapper;

        public GetOrdersBySellerUserNameHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            this.orderRepository = orderRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<OrderResponse>> Handle(GetOrdersBySellerUserNameQuery request, CancellationToken cancellationToken)
        {
            var orderList = await orderRepository.GetOrdersBySellerUserName(request.UserName);
            var response = mapper.Map<IEnumerable<OrderResponse>>(orderList);

            return response;
        }
    }
}