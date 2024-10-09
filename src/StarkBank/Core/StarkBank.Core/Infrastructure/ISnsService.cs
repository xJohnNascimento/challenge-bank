namespace StarkBank.Core.Infrastructure
{
    public interface ISnsService
    {
        public Task<string> PublishAsync(string topicArn, string message);
    }
}
