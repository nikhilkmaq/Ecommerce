using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ECommerceApi.Models
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [JsonIgnore]
        public ApplicationUser? User { get; set; }

        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
