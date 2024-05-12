using EcommerceApi.Models.Coupon;
using EcommerceApi.Models.Order;

namespace EcommerceApi.Responses
{
    public class OrderResponse
    {
        public string Message { get; set; } = string.Empty;
        public Guid OrderId { get; set; } = Guid.Empty;
        public int Amount { get; set; }
        public string TranNo { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone{ get; set; } = string.Empty;
        public string InvoiceAddress { get; set; } = string.Empty;
        public decimal TotalDiscount { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new();
        public DateTime InvoiceDate { get; set; }
        public Guid? TranId { get; set; }
    }
}
