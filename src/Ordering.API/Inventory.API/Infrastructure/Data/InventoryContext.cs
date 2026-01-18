using Inventory.API.Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Infrastructure.Data;

public class InventoryContext :DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<ProductStock> Stocks => Set<ProductStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductStock>().ToContainer("Products").HasNoDiscriminator().HasPartitionKey(x => x.ProductId);

        modelBuilder.AddTransactionalOutboxEntities();
    }
}
