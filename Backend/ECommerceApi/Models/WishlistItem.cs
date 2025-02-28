using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ECommerceApi.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        public int WishlistId { get; set; }

        [JsonIgnore]
        public Wishlist? Wishlist { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
