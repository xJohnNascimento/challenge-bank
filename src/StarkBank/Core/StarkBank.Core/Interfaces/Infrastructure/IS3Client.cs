namespace StarkBank.Domain.Interfaces.Infrastructure
{
    public interface IS3Client
    {
        public Task<Stream> GetObjectContentAsync(string bucketName, string key);
    }
}
