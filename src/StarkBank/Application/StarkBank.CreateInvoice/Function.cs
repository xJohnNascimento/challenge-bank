using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using StarkBank.Core.Infrastructure;
using StarkBank.CreateInvoice.DI;
using StarkBank.Shared.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StarkBank.CreateInvoice;

public class Function
{
    private readonly IStarkBankAuthenticationService _authentication;
    private readonly IS3Service _s3Service;

    public Function()
    {
        var serviceCollection = new ServiceCollection();
        DependencyInjection.ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _authentication = serviceProvider.GetRequiredService<IStarkBankAuthenticationService>();
        _s3Service = serviceProvider.GetRequiredService<IS3Service>();
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        try
        {
            var randomInvoices = new Random().Next(8, 13);
            var invoices = new List<Invoice>();

            for (var i = 0; i < randomInvoices; i++)
            {
                invoices.Add(new Invoice(
                    amount: 200000 + i * 2,
                    name: NameService.Generate(),
                    taxID: CpfService.Generate()
                ));
            }

            var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")
                             ?? throw new InvalidOperationException($"The environment variable S3_BUCKET_NAME is not set");

            var privateKeyName = Environment.GetEnvironmentVariable("PRIVATE_KEY_NAME")
                                 ?? throw new InvalidOperationException($"The environment variable PRIVATE_KEY_NAME is not set");

            var starkBankEnvironment = Environment.GetEnvironmentVariable("STARKBANK_ENVIRONMENT")
                                       ?? throw new InvalidOperationException($"The environment variable STARKBANK_ENVIRONMENT is not set");

            var starkBankProjectId = Environment.GetEnvironmentVariable("STARKBANK_PROJECT_ID")
                                     ?? throw new InvalidOperationException($"The environment variable STARKBANK_PROJECT_ID is not set");

            var privateKey = await _s3Service.GetTextFile(bucketName, privateKeyName);

            await _authentication.InitializeAsync(privateKey, starkBankEnvironment, starkBankProjectId);
            var project = _authentication.GetProject();

            Invoice.Create(invoices, user: project);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error:" + ex.Message);
        }
    }
}
