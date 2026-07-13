using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaEra.API.Data;
using System.Security.Claims;

namespace NovaEra.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return NotFound("Cart is empty");

        return Ok(cart);
    }


    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);


        if (cart == null)
        {
            cart = new Models.Cart
            {
                UserId = userId
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }


        var item = cart.Items
            .FirstOrDefault(i => i.ProductId == productId);


        if (item != null)
        {
            item.Quantity++;
        }
        else
        {
            cart.Items.Add(new Models.CartItem
            {
                ProductId = productId,
                Quantity = 1
            });
        }


        await _context.SaveChangesAsync();

        return Ok(cart);
    }


    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var item = await _context.CartItems.FindAsync(id);

        if (item == null)
            return NotFound();

        _context.CartItems.Remove(item);

        await _context.SaveChangesAsync();

        return Ok();
    }
}