using Amazon;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using StarkBank.Domain.Interfaces.Application.ProcessInvoicePayment;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Infrastructure.Clients;
using StarkBank.Infrastructure.Services;
using StarkBank.ProcessInvoicePayment.Services;

namespace StarkBank.ProcessInvoicePayment.DI
{
    public static class DependencyInjection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStarkBankAuthenticationService, StarkBankAuthenticationService>();

            services.AddSingleton<IS3Client, S3Client>();
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(RegionEndpoint.SAEast1));
            services.AddSingleton<ITransferService, TransferService>();
        }
    }
}
