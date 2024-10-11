using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using StarkBank.CreateInvoice.DI;
using StarkBank.Domain.Interfaces.Application.CreateInvoice;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Domain.Interfaces.Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StarkBank.CreateInvoice;

public class CreateInvoiceFunction
{
    private readonly IStarkBankAuthenticationService _authentication;
    private readonly IS3Service _s3Service;
    private readonly IInvoiceService _invoiceService;
    private readonly IClientGeneratorService _clientGeneratorService;

    public CreateInvoiceFunction()
    {
        var serviceCollection = new ServiceCollection();
        DependencyInjection.ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _authentication = serviceProvider.GetRequiredService<IStarkBankAuthenticationService>();
        _s3Service = serviceProvider.GetRequiredService<IS3Service>();
        _invoiceService = serviceProvider.GetRequiredService<IInvoiceService>();
        _clientGeneratorService = serviceProvider.GetRequiredService<IClientGeneratorService>();
    }

    public CreateInvoiceFunction(
        IStarkBankAuthenticationService authentication,
        IS3Service s3Service,
        IInvoiceService invoiceService,
        IClientGeneratorService clientGeneratorService)
    {
        _authentication = authentication;
        _s3Service = s3Service;
        _invoiceService = invoiceService;
        _clientGeneratorService = clientGeneratorService;
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        try
        {
            var random = new Random();

            var randomInvoices = random.Next(8, 13);
            var randomValue = random.Next(200000, 500001);

            var invoices = new List<Invoice>();

            for (var i = 0; i < randomInvoices; i++)
            {
                invoices.Add(new Invoice(
                    amount: randomValue,
                    name: _clientGeneratorService.GenerateName(),
                    taxID: _clientGeneratorService.GenerateCpf()
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
            var project = _authentication.GetProject() ?? throw new InvalidOperationException("Project could not be created");

            var invoicesCreated = _invoiceService.Create(invoices, user: project);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error:" + ex.Message);
        }
    }
}
