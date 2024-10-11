using StarkBank.Domain.Interfaces.Infrastructure;

namespace StarkBank.Infrastructure.Services
{
    public class StarkBankAuthenticationService : IStarkBankAuthenticationService
    {
        private Project? _project;

        public Task InitializeAsync(string privateKey, string environment, string projectId)
        {
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentException("Private key cannot be null or empty.", nameof(privateKey));

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentException("Environment cannot be null or empty.", nameof(environment));

            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentException("Project ID cannot be null or empty.", nameof(projectId));

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
