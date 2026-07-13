using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaEra.API.Data;
using NovaEra.API.Models;
using System.Security.Claims;

namespace NovaEra.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );


        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);


        if (cart == null || cart.Items.Count == 0)
            return BadRequest("Cart is empty");


        var order = new Order
        {
            UserId = userId,
            Status = "Pending",
            TotalPrice = cart.Items.Sum(
                x => x.Product.Price * x.Quantity
            )
        };


        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price
            });
        }


        _context.Orders.Add(order);


        _context.CartItems.RemoveRange(cart.Items);


        await _context.SaveChangesAsync();


        return Ok(order);
    }



    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );


        var orders = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .ToListAsync();


        return Ok(orders);
    }
}