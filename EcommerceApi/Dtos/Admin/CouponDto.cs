namespace EcommerceApi.Dtos.Admin
{
    public class CouponDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DiscountPercent { get; set; }
        public string Attribute { get; set; } = string.Empty; 
        public string Operator { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public CouponConditionDto OtherCondition { get; set; }
    }
}
