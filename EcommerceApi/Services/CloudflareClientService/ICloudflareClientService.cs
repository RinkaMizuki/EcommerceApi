using Amazon.S3.Model;
using EcommerceApi.Dtos.Upload;

namespace EcommerceApi.Services;

public interface ICloudflareClientService
{
    public Task<PutObjectResponse> UploadImageAsync(UploadDto Upload, string prefix, CancellationToken userCancellationToken);
    public Task<List<S3Object>> GetListObjectAsync(string? prefix, CancellationToken userCancellationToken);
    public Task<GetObjectResponse> GetObjectAsync(string key, CancellationToken userCancellationToken);
    public Task<DeleteObjectResponse> DeleteObjectAsync(string key, CancellationToken userCancellationToken);
}