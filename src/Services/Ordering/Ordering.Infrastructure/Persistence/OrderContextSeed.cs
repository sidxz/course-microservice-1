using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
  public class OrderContextSeed
  {
    public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
    {
      if (!orderContext.Orders.Any())
      {
        orderContext.Orders.AddRange(GetPreconfiguredOrders());
        await orderContext.SaveChangesAsync();
        logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
      }
    }

    private static IEnumerable<Order> GetPreconfiguredOrders()
    {
      return new List<Order>
            {
                new Order() {UserName = "sidx",
                            FirstName = "Siddhant",
                            LastName = "Rath",
                            EmailAddress = "rhymesofsid@gmail.com",
                            AddressLine = "One Downy",
                            Country = "GB",
                            State = "London",
                            CardName = "Siddhant Rath",
                            CardNumber = "1234567890123456",
                            Expiration = "01/23",
                            CVV = "123",
                            TotalPrice = 350 }
            };
    }
  }
}