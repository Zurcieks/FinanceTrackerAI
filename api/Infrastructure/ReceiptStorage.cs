using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace Api.Infrastructure;

public class ReceiptStorage(IAmazonS3 s3, IConfiguration config)
{
    private readonly string _bucket =
        config["Storage:Bucket"] ?? "receipts";

    public async Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct)
    {
        var extension = contentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            _ => "bin"
        };


        var key = $"{Guid.NewGuid()}.{extension}";

        await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType
        }, ct);

        return key;
    }

    public string GetUrl(string key) =>
        s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        try
        {
            await s3.GetObjectMetadataAsync(_bucket, key, ct);
            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        await s3.DeleteObjectAsync(_bucket, key, ct);
    }
}
