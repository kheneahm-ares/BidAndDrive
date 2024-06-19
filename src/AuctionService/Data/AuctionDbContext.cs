using Microsoft.EntityFrameworkCore;

namespace Entities.AuctionService;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }

    //will also create items
    public DbSet<Auction> Auctions { get; set; }
}
