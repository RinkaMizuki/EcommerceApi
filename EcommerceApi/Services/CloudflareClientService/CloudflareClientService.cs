using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using EcommerceApi.Dtos.Upload;
using EcommerceApi.Models.CloudflareR2;
using Microsoft.Extensions.Options;

namespace EcommerceApi.Services;

public class CloudflareClientService : ICloudflareClientService
{
    private readonly CloudflareR2 _options;

    public CloudflareClientService(IOptions<CloudflareR2> options)
    {
        _options = options.Value;
    }

    private AmazonS3Client Authenticate()
    {
        var credentials = new BasicAWSCredentials(_options.accessKey, _options.accessSecret);
        var s3Client = new AmazonS3Client(
            credentials,
            new AmazonS3Config
            {
                ServiceURL = $"https://{_options.accountId}.r2.cloudflarestorage.com"
            });
        return s3Client;
    }

    public async Task<PutObjectResponse> UploadImageAsync(UploadDto Upload, string prefix, CancellationToken userCancellationToken)
    {
        var s3Client = Authenticate();
        var request = new PutObjectRequest()
        {
            BucketName = _options.bucketName,
            Key = $"{prefix}_{Upload.Id}_{Upload.File.FileName}",
            InputStream = Upload.File.OpenReadStream(),
            ContentType = Upload.File.ContentType,
            DisablePayloadSigning = true
        };

        var response = await s3Client.PutObjectAsync(request, userCancellationToken);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK &&
            response.HttpStatusCode != System.Net.HttpStatusCode.Accepted)
        {
            throw new Exception("Upload to Cloudflare R2 failed");
        }

        return response;
    }

    public async Task<List<S3Object>> GetListObjectAsync(string? prefix)
    {
        var s3Client = Authenticate();
        var request = new ListObjectsV2Request
        {
            BucketName = _options.bucketName,
            Prefix = prefix,
        };
        var result = await s3Client.ListObjectsV2Async(request);
        if (!(result.HttpStatusCode == HttpStatusCode.OK))
        {
            throw new Exception("Can't not get list object !!!");
        }

        //Generate PreSignedURL

        //var s3Objects = result.S3Objects.Select((s3Obj) =>
        //{
        //    AWSConfigsS3.UseSignatureVersion4 = true;
        //    var presign = new GetPreSignedUrlRequest()
        //    {
        //        BucketName = s3Obj.BucketName,
        //        Key = s3Obj.Key,
        //        Verb = HttpVerb.GET,
        //        Expires = DateTime.Now.AddMinutes(1)
        //    };
        //    return new S3ObjectDto()
        //    {
        //        Name = s3Obj.Key.ToString(),
        //        PreSignedUrl = s3Client.GetPreSignedURL(presign)
        //    };
        //});
        return result.S3Objects;
    }

    public async Task<GetObjectResponse> GetObjectAsync(string key)
    {
        var s3Client = Authenticate();
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, _options.bucketName);
        if (!bucketExists) throw new Exception($"Bucket {_options.bucketName} does not exist.");
        var s3Object = await s3Client.GetObjectAsync(_options.bucketName, key);
        return s3Object;
    }

    public async Task<DeleteObjectResponse> DeleteObjectAsync(string key, CancellationToken userCancellationToken)
    {
        var s3Client = Authenticate();
        var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, _options.bucketName);
        if (!bucketExists) throw new Exception($"Bucket {_options.bucketName} does not exist.");
        return await s3Client.DeleteObjectAsync(_options.bucketName, key, userCancellationToken);
    }
}