using Amazon.S3;
using Amazon.S3.Model;
using AutoServ.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AutoServ.Infrastructure.Services
{
    public class S3StorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IConfiguration config)
        {
            var aws = config.GetSection("AwsSettings");
            _bucketName = aws["BucketName"];

            _s3Client = new AmazonS3Client(
                aws["AccessKeyId"],
                aws["SecretAccessKey"],
                new AmazonS3Config
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(aws["Region"])
                }
            );
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string folder, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";

            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var key = $"{folder}/{uniqueName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var key = uri.AbsolutePath.TrimStart('/');

                await _s3Client.DeleteObjectAsync(_bucketName, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка видалення з S3: {ex.Message}");
            }
        }
    }
}