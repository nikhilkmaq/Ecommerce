using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.DTOs;
using System.Security.Claims;

namespace ECommerceApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Wishlist
        [HttpGet]
        public async Task<ActionResult<object>> GetWishlist()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            // Map to DTO to avoid circular references
            var wishlistItems = wishlist.WishlistItems?.Select(wi => new WishlistItemDto
            {
                Id = wi.Id,
                ProductId = wi.ProductId,
                ProductName = wi.Product?.Name ?? "Unknown",
                ProductPrice = wi.Product?.Price ?? 0,
                ProductImageUrl = wi.Product?.ImageUrl ?? string.Empty,
                AddedAt = wi.AddedAt
            }).ToList() ?? new List<WishlistItemDto>();

            return new
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId,
                WishlistItems = wishlistItems,
                ItemCount = wishlistItems.Count
            };
        }

        // POST: api/Wishlist/add
        [HttpPost("add")]
        public async Task<ActionResult<object>> AddToWishlist([FromBody] AddToWishlistDto request)
        {
            if (request == null || request.ProductId <= 0)
            {
                return BadRequest(new { message = "Invalid product ID" });
            }

            // Check if the product exists
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {request.ProductId} does not exist" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            // Check if product already exists in wishlist
            var wishlistItem = wishlist.WishlistItems.FirstOrDefault(wi => wi.ProductId == request.ProductId);
            if (wishlistItem != null)
            {
                return Ok(new
                {
                    message = "Product already in wishlist",
                    wishlistItemId = wishlistItem.Id
                });
            }

            // Add new wishlist item
            wishlistItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = request.ProductId
            };
            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                wishlistItemId = wishlistItem.Id,
                productId = request.ProductId,
                message = $"Added product {request.ProductId} to wishlist"
            });
        }

        // DELETE: api/Wishlist/remove/5
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
                return NotFound(new { message = "Wishlist not found" });

            var wishlistItem = wishlist.WishlistItems.FirstOrDefault(wi => wi.ProductId == productId);
            if (wishlistItem == null)
                return NotFound(new { message = $"Product with ID {productId} not found in wishlist" });

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Removed product {productId} from wishlist" });
        }
    }
}
