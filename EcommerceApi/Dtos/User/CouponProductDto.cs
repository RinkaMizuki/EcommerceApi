namespace EcommerceApi.Dtos.User
{
    public class ProductInfo
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
    public class CouponProductDto
    {
        public List<ProductInfo> ListProductInfo { get; set; } = new();
        public string CouponCode { get; set; } = string.Empty;
    }
}
