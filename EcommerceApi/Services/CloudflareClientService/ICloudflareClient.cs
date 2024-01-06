using Amazon.S3.Model;
using EcommerceApi.Dtos.Upload;

namespace EcommerceApi.Services;

public interface ICloudflareClient
{
    public Task<PutObjectResponse> UploadImageAsync(UploadDto Upload, string prefix);
    public Task<IEnumerable<S3ObjectDto>> GetListObjectAsync(string? prefix);
    public Task<GetObjectResponse> GetObjectAsync(string key);
    public Task<DeleteObjectResponse> DeleteObjectAsync(string key);
}