using StarkBank.Domain.Interfaces.Infrastructure;

namespace StarkBank.Infrastructure.Services
{
    public class S3Service(IS3Client s3Client) : IS3Service
    {
        public async Task<string> GetTextFile(string bucketName, string fileKey)
        {
            await using var responseStream = await s3Client.GetObjectContentAsync(bucketName, fileKey);
            using var reader = new StreamReader(responseStream);
            return await reader.ReadToEndAsync();
        }
    }
}
