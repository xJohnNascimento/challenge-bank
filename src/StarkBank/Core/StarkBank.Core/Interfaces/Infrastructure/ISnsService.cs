namespace StarkBank.Domain.Interfaces.Infrastructure
{
    public interface ISnsService
    {
        public Task<string> PublishAsync(string topicArn, string message);
    }
}
