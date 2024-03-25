namespace EcommerceApi.Dtos.User
{
    public class PaymentDto
    {
        //merchant
        public Guid MerchantId { get; set; }
        //destination
        public Guid DestinationId { get; set; }
        //payment
        public int RequiredAmount { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        //order
        public decimal TotalDiscount { get; set; }
        public int TotalQuantity { get; set; }
        public Guid? CouponId { get; set; }
        public int UserId { get; set; }
        public string Note { get; set; } = string.Empty;
        //order detail
        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}
