using Amazon.S3;
using Amazon.S3.Model;
using StarkBank.Core.Infrastructure;

namespace StarkBank.Infrastructure.Services
{
    public class S3Service : IS3Service
    {
        private readonly AmazonS3Client _s3Client = new(Amazon.RegionEndpoint.SAEast1);
        public async Task<string> GetTextFile(string bucketName, string fileKey)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileKey
            };

            using var response = await _s3Client.GetObjectAsync(request);
            await using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream);

            return await reader.ReadToEndAsync();
        }
    }
}
