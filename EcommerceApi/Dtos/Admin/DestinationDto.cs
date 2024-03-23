namespace EcommerceApi.Dtos.Admin
{
    public class DestinationDto
    {
        public string DesLogo { get; set; } = string.Empty;
        public string DesShortName { get; set; } = string.Empty;
        public string DesName { get; set; } = string.Empty;
        public Guid? ParentDestinationId { get; set; }
        public bool IsActive { get; set; }
    }
}
