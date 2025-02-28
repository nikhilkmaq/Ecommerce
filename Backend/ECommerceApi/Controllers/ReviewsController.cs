using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Models;
using ECommerceApi.DTOs;
using System.Security.Claims;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reviews/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {productId} not found" });
            }

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    ProductId = r.ProductId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        // GET: api/Reviews/product/5/average
        [HttpGet("product/{productId}/average")]
        public async Task<ActionResult<object>> GetProductAverageRating(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {productId} not found" });
            }

            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (reviews.Count == 0)
            {
                return Ok(new
                {
                    ProductId = productId,
                    AverageRating = 0,
                    ReviewCount = 0
                });
            }

            var averageRating = reviews.Average(r => r.Rating);
            var reviewCount = reviews.Count;

            return Ok(new
            {
                ProductId = productId,
                AverageRating = averageRating,
                ReviewCount = reviewCount
            });
        }

        // POST: api/Reviews
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Review>> AddReview(AddReviewDto reviewDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            var product = await _context.Products.FindAsync(reviewDto.ProductId);
            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {reviewDto.ProductId} not found" });
            }

            // Check if user has already reviewed this product
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == reviewDto.ProductId);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.Rating = reviewDto.Rating;
                existingReview.Comment = reviewDto.Comment;
                existingReview.CreatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Review updated successfully",
                    reviewId = existingReview.Id
                });
            }

            // Create new review
            var review = new Review
            {
                UserId = userId,
                ProductId = reviewDto.ProductId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductReviews),
                new { productId = review.ProductId },
                new { message = "Review added successfully", reviewId = review.Id });
        }

        // DELETE: api/Reviews/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new { message = $"Review with ID {id} not found" });
            }

            // Check if the user is the owner of the review or an admin
            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review deleted successfully" });
        }
    }
}
