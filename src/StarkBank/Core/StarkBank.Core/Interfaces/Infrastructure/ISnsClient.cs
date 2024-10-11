namespace StarkBank.Domain.Interfaces.Infrastructure
{
    public interface ISnsClient
    {
        public Task<string> PublishAsync(string topicArn, string message);
    }
}
