using Amazon;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using StarkBank.CreateInvoice.Services;
using StarkBank.Domain.Interfaces.Application.CreateInvoice;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Domain.Interfaces.Shared;
using StarkBank.Infrastructure.Clients;
using StarkBank.Infrastructure.Services;
using StarkBank.Shared.Services;
using System.Diagnostics.CodeAnalysis;

namespace StarkBank.CreateInvoice.DI
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStarkBankAuthenticationService, StarkBankAuthenticationService>();

            services.AddSingleton<IS3Client, S3Client>();
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(RegionEndpoint.SAEast1));


            services.AddSingleton<IInvoiceService, InvoiceService>();

            services.AddSingleton<IClientGeneratorService, ClientGeneratorService>();
        }
    }
}
