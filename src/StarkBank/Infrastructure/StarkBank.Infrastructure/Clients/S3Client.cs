using Amazon.S3;
using Amazon.S3.Model;
using StarkBank.Domain.Interfaces.Infrastructure;

namespace StarkBank.Infrastructure.Clients
{
    public class S3Client(IAmazonS3 s3Client) : IS3Client
    {
        public async Task<Stream> GetObjectContentAsync(string bucketName, string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
    }
}
