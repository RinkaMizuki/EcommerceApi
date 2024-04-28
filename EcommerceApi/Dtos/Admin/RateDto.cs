using EcommerceApi.Models.Product;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApi.Dtos.Admin
{
    public class RateDto
    {
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Star { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
