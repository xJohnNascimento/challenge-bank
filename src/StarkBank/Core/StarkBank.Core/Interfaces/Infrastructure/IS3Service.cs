namespace StarkBank.Domain.Interfaces.Infrastructure
{
    public interface IS3Service
    {
        public Task<string> GetTextFile(string bucketName, string fileKey);
    }
}
