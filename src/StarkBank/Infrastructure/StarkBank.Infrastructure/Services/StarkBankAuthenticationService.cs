using StarkBank.Core.Infrastructure;

namespace StarkBank.Infrastructure.Services
{
    public class StarkBankAuthenticationService : IStarkBankAuthenticationService
    {
        private Project? _project;

        public Task InitializeAsync(string privateKey, string environment, string projectId)
        {
            _project = new Project(
                environment: environment,
                id: projectId,
                privateKey: privateKey
            );
            return Task.CompletedTask;
        }

        public Project? GetProject()
        {
            return _project;
        }
    }
}
