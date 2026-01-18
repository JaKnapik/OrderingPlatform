using Inventory.API.Infrastructure.Data;
using MassTransit;

namespace Inventory.API.Features;

public record OrderCreatedEvent(Guid OrderId, decimal TotalAmout, string UserId, string storeCode);

public class OrderCreatedConsumer(InventoryContext dbContext, ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Processing OrderCreatedEvent for Order: {OrderId}", message.OrderId);

        //Normally here would be some kind of bussiness logic - reduce store count etc. For learning purposes it's irrelevant, we only wanted to try mass transit/cosmosDB in practice

        await dbContext.SaveChangesAsync();
    }
}
