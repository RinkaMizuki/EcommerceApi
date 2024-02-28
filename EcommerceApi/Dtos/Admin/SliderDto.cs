namespace EcommerceApi.Dtos.Admin
{
    public class SliderDto
    {
        public IFormFile FormFile { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

    }
}
