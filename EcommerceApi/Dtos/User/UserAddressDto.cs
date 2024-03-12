namespace EcommerceApi.Dtos.User
{
    public class UserAddressDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public bool IsDeliveryAddress { get; set; }
        public bool IsPickupAddress { get; set; }
        public bool IsReturnAddress { get; set; }
    }
}
