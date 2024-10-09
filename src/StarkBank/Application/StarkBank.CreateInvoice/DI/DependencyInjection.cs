using Microsoft.Extensions.DependencyInjection;
using StarkBank.Core.Infrastructure;
using StarkBank.Infrastructure.Services;

namespace StarkBank.CreateInvoice.DI
{
    public static class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStarkBankAuthenticationService, StarkBankAuthenticationService>();
            services.AddSingleton<IS3Service, S3Service>();
        }
    }
}
