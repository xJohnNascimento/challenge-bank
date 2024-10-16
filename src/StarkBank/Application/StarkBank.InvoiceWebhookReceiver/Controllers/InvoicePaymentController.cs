using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using StarkBank.Domain.Interfaces.Infrastructure;

namespace StarkBank.InvoiceWebhookReceiver.Controllers;

[ApiController]
public class InvoicePaymentController(ILogger<InvoicePaymentController> logger, ISnsService snsService) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<APIGatewayProxyResponse> ReceiveWebhook([FromBody] object? payload)
    {
        string? clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(clientIp) && HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrEmpty(clientIp))
            {
                clientIp = clientIp.Split(',').First().Trim();
            }
        }

        logger.LogInformation($"Request IP: {clientIp}");

        try
        {
            if (string.IsNullOrEmpty(payload?.ToString()))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 422,
                    Body = "{\"error\": \"The payload was empty or invalid\"}",
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN")
                           ?? throw new InvalidOperationException("The environment variable TOPIC_ARN is not set");

            await snsService.PublishAsync(topicArn, payload.ToString()!);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "{\"message\": \"Webhook processed successfully\"}",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing webhook");

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = "{\"error\": \"Internal Server Error\"}",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
