using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaEra.API.Data;
using NovaEra.API.DTOs;

namespace NovaEra.API.Controllers;

[ApiController]
[Route("api/Admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync();

        return Ok(orders);
    }


    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound("Order not found");

        order.Status = request.Status;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Order status updated successfully",
            order.Id,
            order.Status
        });
    }
}