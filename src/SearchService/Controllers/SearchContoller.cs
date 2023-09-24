using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using ZstdSharp.Unsafe;

namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchContoller : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams){
        PagedSearch<Item, Item> query = DB.PagedSearch<Item, Item>();

        if(!string.IsNullOrEmpty(searchParams.SearchTerm)){
            query.Match(Search.Full, searchParams.SearchTerm);

        }

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(i => i.Ascending(a => a.Make)),
            "new" => query.Sort(i => i.Descending(a => a.CreatedAt)),
            _ => query.Sort(i => i.Ascending(a => a.AuctionEnd))
        };

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(a => a.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(a => a.AuctionEnd < DateTime.UtcNow.AddHours(6) && a.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(a => a.AuctionEnd > DateTime.UtcNow)
        };

        if(!string.IsNullOrEmpty(searchParams.Seller)){
            query.Match(a => a.Seller == searchParams.Seller);
        }

        if(!string.IsNullOrEmpty(searchParams.Winner)){
            query.Match(a => a.Winner == searchParams.Winner);
        }

        var result = await query.ExecuteAsync();

        return Ok(new {
            totalCount = result.TotalCount,
            pageCount = result.PageCount,
            results = result.Results
        });
    }
}
