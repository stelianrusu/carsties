using ActionService.Data;
using ActionService.DTOs;
using ActionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext context;
    private readonly IMapper mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(){
        var auctions = await context.Auctions.Include(a => a.Item)
        .OrderBy(a => a.Item.Make).ToListAsync();

        return mapper.Map<List<AuctionDto>>(auctions);

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id){
        var auction = await context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);

        if(auction == null) return NotFound();

        return mapper.Map<AuctionDto>(auction);

    }

    [HttpPost]
    public async  Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto){
        var action = mapper.Map<Auction>(auctionDto);
        action.Seller = "test";

        context.Auctions.Add(action);

        var result = await context.SaveChangesAsync() > 0;
        if(!result)
            return BadRequest("Could not save changes to db");
        
        return CreatedAtAction(nameof(GetAuctionById), new {action.Id}, mapper.Map<AuctionDto>(action));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto){
        var auction = await context.Auctions.Include(a => a.Item)
        .FirstOrDefaultAsync(a => a.Id == id);

        if(auction ==  null)
            return NotFound();
        
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await context.SaveChangesAsync() > 0;
        if(result) return Ok();

        return BadRequest("Problem saving changes");

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id){
        var auction = context.Auctions.FirstOrDefault(a => a.Id == id);

        if(auction == null) return NotFound();
        context.Auctions.Remove(auction);

        await context.SaveChangesAsync();

        return Ok();
    }
}
