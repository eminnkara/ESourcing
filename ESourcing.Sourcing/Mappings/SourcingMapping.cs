using AutoMapper;
using Esourcing.Sourcing.Entities;
using EventBusRabbitMQ.Events;

namespace ESourcing.Sourcing.Mappings
{
    public class SourcingMapping : Profile
    {
        public SourcingMapping()
        {
            CreateMap<OrderCreateEvent, Bid>().ReverseMap();
        }
    }
}