using EcommerceApi.Dtos.Admin;
using EcommerceApi.Models.Slider;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Services.SliderService
{
    public interface ISliderService
    {
        public Task<Slider> PostSliderAsync(SliderDto sliderDto, HttpRequest request, CancellationToken cancellationToken);
        public Task<Slider> UpdateSliderAsync(SliderDto sliderDto, Guid sliderId, HttpRequest request, string userName, CancellationToken cancellationToken);
        public Task<bool> DeleteSliderAsync(Guid sliderId, CancellationToken userCancellationToken);
        public Task<List<Slider>> GetListSliderAsync(CancellationToken userCancellationToken);
        public Task<Slider> GetSliderByIdAsync(Guid sliderId, CancellationToken userCancellationToken);
        public Task<FileStreamResult> GetImageAsync(string imageUrl, CancellationToken userCancellationToken);
    }
}
