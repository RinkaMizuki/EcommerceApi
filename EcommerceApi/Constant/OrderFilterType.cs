namespace EcommerceApi.Constant
{
    public static class OrderFilterType
    {
        public const string Status = "status";
        public const string UserId = "userId";
        public const string OrderId = "id";
        public const string Before = "orderedBefore";
        public const string Since = "orderedSince";
        public const string Returned = "returned";
        public const string Shipped = "shipped";
        public const string Cancelled = "cancelled";
        public const string Ordered = "ordered";
        public const string Delivered = "delivered";
        public const string MinAmount = "minAmount";
        public const string OrderedOrDelivered = "ordered || delivered";
    }
}
