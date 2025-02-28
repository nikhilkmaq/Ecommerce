using Microsoft.AspNetCore.Identity;

namespace ECommerceApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Address { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Cart? Cart { get; set; }
        public Wishlist? Wishlist { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}