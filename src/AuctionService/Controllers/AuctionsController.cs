using AutoMapper;
using Contracts;
using DTOs.AuctionService;
using Entities.AuctionService;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers.AuctionService;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext dbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> Get()
    {
        var auctions = await _dbContext.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> Get(Guid id)
    {
        var auction = await _dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> Post([FromBody] CreateAuctionDto dto)
    {
        var auction = _mapper.Map<Auction>(dto);
        auction.Seller = "test";
        _dbContext.Auctions.Add(auction);

        //because of theoutbox config, we can add this to the batched transaction
        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _dbContext.SaveChangesAsync();

        if (result == 0)
        {
            return BadRequest();
        }
        else
        {
            //show location in header of where it's created
            return CreatedAtAction(nameof(Get), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AuctionDto>> Put(Guid id, [FromBody] UpdateAuctionDto dto)
    {
        var auction = await _dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }

        //TODO: check if user is the seller

        auction.Item.Make = dto.Make ?? auction.Item.Make;
        auction.Item.Model = dto.Model ?? auction.Item.Model;
        auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
        auction.Item.Color = dto.Color ?? auction.Item.Color;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _dbContext.SaveChangesAsync();
        if (result == 0)
        {
            return BadRequest();
        }
        else
        {
            return Ok(_mapper.Map<AuctionDto>(auction));
        }
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var auction = await _dbContext.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        //todo: check if user is the seller

        await _publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});

        
        _dbContext.Auctions.Remove(auction);
        var result = await _dbContext.SaveChangesAsync();
        if (result == 0)
        {
            return BadRequest();
        }
        else
        {
            return Ok();
        }
    }
}
