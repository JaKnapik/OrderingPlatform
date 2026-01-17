using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Domain;
using Ordering.API.Infrastructure.Auth;

namespace Ordering.API.Infrastructure.Data;

public class OrderingContext: IdentityDbContext<ApplicationUser>
{
	public OrderingContext(DbContextOptions<OrderingContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Store> Stores => Set<Store>();
	public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
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
        builder.Entity<Store>()
            .HasIndex(s => s.StoreCode)
            .IsUnique();

        builder.Entity<Order>()
            .HasOne(o => o.Store)
            .WithMany(s => s.Orders)
            .HasPrincipalKey(s => s.StoreCode)
            .HasForeignKey(o => o.StoreCode);
        
        builder.Entity<Order>()
            .Property<string>("LastModifiedBy")
            .IsRequired(false);

        builder.Entity<Order>()
            .Property<DateTime>("LastModifiedAt")
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);
        builder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);
        builder.AddTransactionalOutboxEntities();
    }
}
