﻿namespace EcommerceApi.Dtos.Admin
{
    public class ProductDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public bool Status { get; set; }
        public bool Hot { get; set; }
        public bool FlashSale { get; set; }
        public bool Upcoming { get; set; }
        public int Quantity { get; set; }
        public int Return { get; set; }
        public int CategoryId { get; set; }
        public string? ColorCode { get; set; } = string.Empty;
        public IFormFile[] Files { get; set; }
    }
}
