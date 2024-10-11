using StarkBank.Domain.Interfaces.Infrastructure;

namespace StarkBank.Infrastructure.Services
{
    public class SnsService(ISnsClient snsClient) : ISnsService
    {
        public async Task<string> PublishAsync(string topicArn, string message)
        {
            return await snsClient.PublishAsync(topicArn, message);
        }
    }
}
