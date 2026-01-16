using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Infrastructure.Auth;

namespace Ordering.API.Infrastructure.Data;

public class OrderingContext: IdentityDbContext<ApplicationUser>
{
	public OrderingContext(DbContextOptions<OrderingContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .OwnsMany(u => u.RefreshTokens, rt =>
            {
                rt.WithOwner().HasForeignKey("UserId");
                rt.Property<int>("Id");
                rt.HasKey("Id");
            });
    }
}
