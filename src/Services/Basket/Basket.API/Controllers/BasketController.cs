using System.Net;
using Basket.API.Entities;
using Basket.API.GrpcService;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers
{
  [ApiController]
  [Route("api/v1/[controller]")]
  public class BasketController : ControllerBase
  {
    private readonly IBasketRepository _repository;
    private readonly DiscountGrpcService _discountGrpcService;

    public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService)
    {
      _repository = repository ?? throw new ArgumentNullException(nameof(repository));
      _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
    }

    [HttpGet("{userName}", Name = "GetBasket")]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
    {
      var basket = await _repository.GetBasket(userName);
      return Ok(basket ?? new ShoppingCart(userName));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
    {

      // Calculate latest prices of product into shopping cart
      // consume Discount Grpc (not directly from here, but encapsulate it into Basket.Grpc)

      foreach (var item in basket.Items)
      {
        var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
        item.Price -= coupon.Amount;
      }

      return Ok(await _repository.UpdateBasket(basket));
    }

    [HttpDelete("{userName}", Name = "DeleteBasket")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteBasket(string userName)
    {
      await _repository.DeleteBasket(userName);
      return Ok();
    }

    [Route("[action]")]
    [HttpPost]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
    {
      // get existing basket with total price
      var basket = await _repository.GetBasket(basketCheckout.UserName);
      if (basket == null)
      {
        return BadRequest();
      }

      // Create basketCheckoutEvent -- Set TotalPrice on basketCheckout eventMessage

      // send checkout event to rabbitmq

      // remove the basket
      await _repository.DeleteBasket(basket.UserName);

      return Accepted();
    }

  }
}