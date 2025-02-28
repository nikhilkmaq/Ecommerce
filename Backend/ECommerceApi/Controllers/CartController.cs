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
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Map to DTO to avoid circular references
            var cartItems = cart.CartItems?.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                CartId = ci.CartId,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? "Unknown",
                ProductPrice = ci.Product?.Price ?? 0,
                ProductImageUrl = ci.Product?.ImageUrl ?? string.Empty,
                Quantity = ci.Quantity
            }).ToList() ?? new List<CartItemDto>();

            return new
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cartItems,
                ItemCount = cartItems.Sum(i => i.Quantity),
                TotalPrice = cartItems.Sum(i => i.Total)
            };
        }

        [HttpPost("items")]
        public async Task<ActionResult<object>> AddToCart([FromBody] AddToCartDto request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest(new { message = "Invalid product ID or quantity" });
            }

            // First check if the product exists
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {request.ProductId} does not exist" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += request.Quantity;
            }

            await _context.SaveChangesAsync();

            // Return a simple response without circular references
            return Ok(new
            {
                cartItemId = cartItem.Id,
                productId = request.ProductId,
                quantity = cartItem.Quantity,
                message = $"Added {request.Quantity} of product {request.ProductId} to cart"
            });
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound();

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
