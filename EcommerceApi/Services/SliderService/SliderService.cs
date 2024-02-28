using EcommerceApi.Dtos.Admin;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Models;
using EcommerceApi.Models.Slider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace EcommerceApi.Services.SliderService
{
    public class SliderService : ISliderService
    {
        private readonly EcommerceDbContext _context;
        private readonly ICloudflareClientService _cloudflareClient;
        public SliderService(EcommerceDbContext context, ICloudflareClientService cloudflareClient) { 
            _context = context;
            _cloudflareClient = cloudflareClient;
        }
        public async Task<bool> DeleteSliderAsync(Guid sliderId, CancellationToken userCancellationToken)
        {
            try
            {
                var sliderDelete = await _context
                                                .Sliders
                                                .Where(s => s.SilderId == sliderId)
                                                .FirstOrDefaultAsync(userCancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Slider not found.");
                _context.Sliders.Remove(sliderDelete);
                await _context.SaveChangesAsync(userCancellationToken);
                await _cloudflareClient.DeleteObjectAsync($"productImage_{sliderDelete.SilderId}_{sliderDelete.Image}",
                userCancellationToken);

                return true;
            }
            catch(Exception ex) {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<List<Slider>> GetListCouponAsync(CancellationToken userCancellationToken)
        {
            try
            {
                var listSlider = await _context
                                                .Sliders
                                                .AsNoTracking()
                                                .ToListAsync(userCancellationToken);
                return listSlider;
            }
            catch(Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Slider> PostSliderAsync(SliderDto sliderDto, HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var newSlider = new Slider()
                {
                    SilderId = Guid.NewGuid(),
                    Description = sliderDto.Description,
                    Title = sliderDto.Title,
                    Image = sliderDto.FormFile.FileName,
                    ModifiedAt = DateTime.Now,
                };
                newSlider.Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/slider/preview?sliderImage=sliderImage_{newSlider.SilderId}_{newSlider.Image}";
                

                await _context.Sliders.AddAsync(newSlider, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await _cloudflareClient.UploadImageAsync(new UploadDto()
                {
                    Id = newSlider.SilderId,
                    File = sliderDto.FormFile,
                }, prefix: "sliderImage", cancellationToken);

                return newSlider;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<Slider> UpdateSliderAsync(SliderDto sliderDto, Guid sliderId, HttpRequest request, string userName,CancellationToken cancellationToken)
        {
            try
            {
                var sliderUpdate = await _context
                                                .Sliders
                                                .Where(s => s.SilderId == sliderId)
                                                .FirstOrDefaultAsync(cancellationToken)
                                                ?? throw new HttpStatusException(HttpStatusCode.NotFound, "Slider not found.");
                var oldImage = sliderUpdate.Image;

                sliderUpdate.Title = sliderDto.Title;
                sliderUpdate.Description = sliderDto.Description;
                sliderUpdate.ModifiedBy = userName;
                sliderUpdate.ModifiedAt = DateTime.Now;
                sliderUpdate.Image = sliderDto.FormFile.FileName;
                sliderUpdate.Url = $"{request.Scheme}://{request.Host}/api/v1/Admin/slider/preview?sliderImage=sliderImage_{sliderUpdate.SilderId}_{sliderUpdate.Image}";

                if(sliderDto.FormFile.FileName != oldImage)
                {
                    await _cloudflareClient.DeleteObjectAsync(
                            $"productImage_{sliderId}_{sliderUpdate.Image}", cancellationToken);

                    await _cloudflareClient.UploadImageAsync(new UploadDto()
                    {
                        Id = sliderUpdate.SilderId,
                        File = sliderDto.FormFile,
                    }, prefix: "sliderImage", cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);

                return sliderUpdate;
            }
            catch (Exception ex)
            {
                throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public async Task<FileStreamResult> GetImageAsync(string imageUrl, CancellationToken userCancellationToken)
        {
            var response = await _cloudflareClient.GetObjectAsync(imageUrl, userCancellationToken);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return new FileStreamResult(response.ResponseStream, response.Headers.ContentType)
                {
                    FileDownloadName = imageUrl
                };
            }

            throw new HttpStatusException(response.HttpStatusCode, "Image not found.");
        }
    }
}
