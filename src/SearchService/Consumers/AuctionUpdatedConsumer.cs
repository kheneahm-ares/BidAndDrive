using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            var item = _mapper.Map<Item>(context.Message);

            var result = await DB.Update<Item>()
            .Match(a => a.ID == context.Message.Id) //find item
            .ModifyOnly( x => new // only modify these properties
            {
                x.Color,
                x.Make,
                x.Model,
                x.Mileage
            }, item).ExecuteAsync();

            if (!result.IsAcknowledged) throw new MessageException(typeof(AuctionUpdated), "Could not update item in mongo");
        }
        
    }
}