using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ECommerceApi.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [JsonIgnore]
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
