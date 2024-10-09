using Microsoft.AspNetCore.Mvc;
using StarkBank.Core.Infrastructure;

namespace StarkBank.InvoiceWebhookReceiver.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicePaymentController(ILogger<InvoicePaymentController> logger, ISnsService snsService, IConfiguration configuration) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveWebhook([FromBody] object? payload)
    {
        try
        {
            if (string.IsNullOrEmpty(payload?.ToString()))
            {
                return StatusCode(422, new { error = "The payload was empty or invalid" });
            }

            var topicArn = (configuration["TOPIC_ARN"] ?? Environment.GetEnvironmentVariable("TOPIC_ARN")) ?? throw new InvalidOperationException("The environment variable TOPIC_ARN is not set");

            _ = await snsService.PublishAsync(topicArn, payload.ToString()!);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing webhook");

            return StatusCode(500, new { error = "Internal Server Error" });
        }
    }
}
