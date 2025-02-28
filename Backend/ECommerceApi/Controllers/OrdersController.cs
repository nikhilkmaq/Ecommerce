using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;
using System.Security.Claims;
using ECommerceApi.DTOs;

namespace ECommerceApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var orders = isAdmin
                ? await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ToListAsync()
                : await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

            return orders;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (!isAdmin && order.UserId != userId)
                return Forbid();

            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart?.CartItems == null || !cart.CartItems.Any())
                return BadRequest("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price,
                    Price = ci.Product.Price * ci.Quantity
                }).ToList()
            };

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Price);

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusUpdate)
        {
            if (statusUpdate == null)
            {
                return BadRequest(new { message = "Status update data is missing" });
            }

            // Check if the user is admin
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized(new { message = "Admin access required" });
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found" });

            order.Status = statusUpdate.Status;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = $"Order status updated to {statusUpdate.Status}",
                    orderId = id,
                    status = statusUpdate.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to update order status: {ex.Message}" });
            }
        }
    }
}
