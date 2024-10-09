namespace StarkBank.Core.Infrastructure
{
    public interface IStarkBankAuthenticationService
    {
        public Task InitializeAsync(string privateKey, string environment, string projectId);
        public Project? GetProject();
    }
}
