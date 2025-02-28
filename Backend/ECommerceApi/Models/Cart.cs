using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ECommerceApi.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [JsonIgnore]
        public ApplicationUser? User { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
