using Amazon.S3;
using Amazon.S3.Model;
using System.Text.RegularExpressions;
using EasyOrderCs.Services.Interfaces;

namespace EasyOrderCs.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public FileUploadService(IConfiguration configuration)
    {
        var endpoint = configuration["R2_ENDPOINT"];
        var accessKeyId = configuration["R2_ACCESS_KEY_ID"];
        var secretAccessKey = configuration["R2_SECRET_ACCESS_KEY"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(secretAccessKey))
        {
            throw new InvalidOperationException(
                "R2 configuration is incomplete. Please check your environment variables: R2_ENDPOINT, R2_ACCESS_KEY_ID, R2_SECRET_ACCESS_KEY");
        }

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
        _bucketName = configuration["R2_BUCKET_NAME"] ?? throw new InvalidOperationException("R2_BUCKET_NAME is required");
        _publicUrl = configuration["R2_PUBLIC_URL"] ?? throw new InvalidOperationException("R2_PUBLIC_URL is required");
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder = "")
    {
        var fileExtension = Path.GetExtension(file.FileName).TrimStart('.');
        var fileName = $"{folder}{(string.IsNullOrEmpty(folder) ? "" : "/")}{Guid.NewGuid()}.{fileExtension}";

        using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = stream,
            ContentType = file.ContentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await _s3Client.PutObjectAsync(request);

        return $"{_publicUrl.TrimEnd('/')}/{fileName}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            if (string.IsNullOrEmpty(fileName))
                return;

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(request);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - file deletion is not critical
            Console.Error.WriteLine($"Error deleting file: {ex.Message}");
        }
    }

    public string GetFileUrl(string fileName)
    {
        return $"{_publicUrl.TrimEnd('/')}/{fileName}";
    }
}

