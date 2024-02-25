namespace EcommerceApi.Dtos.Admin
{
    public class CouponDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DiscountPercent { get; set; }
        public List<CouponConditionDto> OtherConditions { get; set; } = new();
    }
}
