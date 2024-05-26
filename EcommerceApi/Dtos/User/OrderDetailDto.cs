namespace EcommerceApi.Dtos.User
{
    public class OrderDetailDto
    {
        public Guid ProductId { get; set; }
        public int DiscountProduct { get; set; }
        public string Color { get; set; } = string.Empty;
        public int PriceProduct { get; set; }
        public int QuantityProduct { get; set; }
    }
}
