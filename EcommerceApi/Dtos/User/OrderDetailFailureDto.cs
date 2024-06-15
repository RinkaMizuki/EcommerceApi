namespace EcommerceApi.Dtos.User
{
    public class OrderDetailFailureDto
    {
        public int PriceProduct {  get; set; }
        public Guid ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int QuantityProduct { get; set; }
        public int StockQuantity {  get; set; }
        public int DiscountProduct { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
