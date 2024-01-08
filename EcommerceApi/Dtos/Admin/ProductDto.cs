using EcommerceApi.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Dtos.Admin
{
    public class ProductDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public Boolean Status { get; set; }
        public Boolean Hot { get; set; }
        public Boolean FlashSale { get; set; }
        public Boolean Upcoming { get; set; }
        public int Quantity { get; set; }
        public int Return { get; set; }
        public int CategoryId { get; set; }
        public string ColorCode { get; set; } = string.Empty;
        public IFormFile[] Files { get; set; }
    }
}
