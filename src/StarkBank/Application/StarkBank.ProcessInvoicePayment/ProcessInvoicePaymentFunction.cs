using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Domain.Models;
using StarkBank.Domain.Models.InvoiceWebhook;
using StarkBank.ProcessInvoicePayment.DI;
using System.Text.Json;
using StarkBank.Domain.Interfaces.Application.ProcessInvoicePayment;
using StarkBank.Error;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StarkBank.ProcessInvoicePayment;

public class ProcessInvoicePaymentFunction
{
    private readonly IStarkBankAuthenticationService _authentication;
    private readonly IS3Service _s3Service;
    private readonly ITransferService _transferService;
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
    public ProcessInvoicePaymentFunction()
    {
        var serviceCollection = new ServiceCollection();
        DependencyInjection.ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _authentication = serviceProvider.GetRequiredService<IStarkBankAuthenticationService>();
        _s3Service = serviceProvider.GetRequiredService<IS3Service>();
        _transferService = serviceProvider.GetRequiredService<ITransferService>();
    }

    public ProcessInvoicePaymentFunction(IStarkBankAuthenticationService authentication, IS3Service s3Service, ITransferService transferService)
    {
        _authentication = authentication;
        _s3Service = s3Service;
        _transferService = transferService;
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing message {message.Body}");

            var snsNotification = JsonSerializer.Deserialize<SnsNotificationModel>(message.Body);

            if (snsNotification?.Message is null)
            {
                context.Logger.LogError("SQS message was null.");
                return;
            }

            var webhookEvent = JsonSerializer.Deserialize<WebhookEventModel>(snsNotification.Message, _options);

            if (webhookEvent?.Event?.Log?.Invoice?.Amount is null ||
                webhookEvent?.Event?.Log?.Invoice?.TaxId is null ||
                webhookEvent?.Event?.Log?.Invoice?.Name is null)
            {
                context.Logger.LogError($"Invoice is missing required fields: " +
                                        $"Amount = {webhookEvent?.Event?.Log?.Invoice?.Amount}, " +
                                        $"TaxID = {webhookEvent?.Event?.Log?.Invoice?.TaxId}, " +
                                        $"Name = {webhookEvent?.Event?.Log?.Invoice?.Name}");
                return;
            }

            if (webhookEvent?.Event?.Log?.Invoice?.Status == "paid")
            {
                var bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")
                                 ?? throw new InvalidOperationException(
                                     $"The environment variable S3_BUCKET_NAME is not set");

                var privateKeyName = Environment.GetEnvironmentVariable("PRIVATE_KEY_NAME")
                                     ?? throw new InvalidOperationException(
                                         $"The environment variable PRIVATE_KEY_NAME is not set");

                var starkBankEnvironment = Environment.GetEnvironmentVariable("STARKBANK_ENVIRONMENT")
                                           ?? throw new InvalidOperationException(
                                               $"The environment variable STARKBANK_ENVIRONMENT is not set");

                var starkBankProjectId = Environment.GetEnvironmentVariable("STARKBANK_PROJECT_ID")
                                         ?? throw new InvalidOperationException(
                                             $"The environment variable STARKBANK_PROJECT_ID is not set");

                var privateKey = await _s3Service.GetTextFile(bucketName, privateKeyName);

                await _authentication.InitializeAsync(privateKey, starkBankEnvironment, starkBankProjectId);
                var project = _authentication.GetProject() ?? throw new InvalidOperationException("Project could not be created");

                var amount = webhookEvent.Event.Log.Invoice.Amount - webhookEvent.Event.Log.Invoice.Fee ?? 0;
                var transfer = _transferService.CreateTransfer(amount, project);
            }

            context.Logger.LogInformation($"Processed successfully message {message.Body}");

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error:" + ex.Message);
            throw new InternalServerError();
        }
    }
}