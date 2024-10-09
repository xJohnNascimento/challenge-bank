namespace StarkBank.Core.Infrastructure
{
    public interface IS3Service
    {
        public Task<string> GetTextFile(string bucketName, string fileKey);
    }
}
