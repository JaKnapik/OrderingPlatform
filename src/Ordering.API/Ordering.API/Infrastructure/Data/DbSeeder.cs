using Ordering.API.Domain;

namespace Ordering.API.Infrastructure.Data;

public static class DbSeeder
{
	public static async Task SeedAsync(OrderingContext context)
	{
		if (!context.Stores.Any())
		{
			context.Stores.AddRange(
				new Store
				{
					Id = Guid.NewGuid(),
					Name = "Jaworzno",
					StoreCode = "JAW-01"
				},
				new Store
				{
					Id = Guid.NewGuid(),
					Name = "Pszczyna",
					StoreCode = "PNY-01"
				}
				);
			await context.SaveChangesAsync();
		}
	}
}
