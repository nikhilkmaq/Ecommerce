using ECommerceApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
