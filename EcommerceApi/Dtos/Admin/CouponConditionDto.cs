namespace EcommerceApi.Dtos.Admin
{
    public class CouponConditionDto
    {
        public string OtherAttribute { get; set; } = string.Empty;
        public string OtherOperator { get; set; } = string.Empty;
        public decimal OtherValue { get; set; }
    }
}
