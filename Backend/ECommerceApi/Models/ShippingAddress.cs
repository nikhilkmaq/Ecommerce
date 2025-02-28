using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.Models
{
    public class ShippingAddress
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
