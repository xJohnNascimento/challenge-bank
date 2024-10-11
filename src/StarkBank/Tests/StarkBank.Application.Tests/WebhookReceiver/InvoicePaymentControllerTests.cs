using Castle.Core.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.InvoiceWebhookReceiver.Controllers;

namespace StarkBank.Application.Tests.WebhookReceiver
{
    public class InvoicePaymentControllerTests
    {
        private readonly InvoicePaymentController _controller;
        private readonly Mock<ILogger<InvoicePaymentController>> _mockLogger;
        private readonly Mock<ISnsService> _mockSnsService;
        private readonly DefaultHttpContext _httpContext;

        public InvoicePaymentControllerTests()
        {
            _mockLogger = new Mock<ILogger<InvoicePaymentController>>();
            _mockSnsService = new Mock<ISnsService>();

            _controller = new InvoicePaymentController(_mockLogger.Object, _mockSnsService.Object);
            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = _httpContext };
        }

        [Fact]
        public async Task ReceiveWebhook_Should_Return_200_When_ValidPayload()
        {
            const string validPayload = "{ \"event\": \"payment\" }";

            Environment.SetEnvironmentVariable("TOPIC_ARN", "arn");

            var response = await _controller.ReceiveWebhook(validPayload);

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("{\"message\": \"Webhook processed successfully\"}", response.Body);
        }

        [Fact]
        public async Task ReceiveWebhook_Should_Return_422_When_EmptyPayload()
        {
            var emptyPayload = string.Empty;

            var response = await _controller.ReceiveWebhook(emptyPayload);

            Assert.NotNull(response);
            Assert.Equal(422, response.StatusCode);
            Assert.Equal("{\"error\": \"The payload was empty or invalid\"}", response.Body);
        }

        [Fact]
        public async Task ReceiveWebhook_Should_Return_500_When_ExceptionThrown()
        {
            const string validPayload = "{ \"event\": \"payment\" }";
            const string topicArn = "arn:aws:sns:region:account-id:topic-name";

            _mockSnsService.Setup(s => s.PublishAsync(topicArn, validPayload))
                .ThrowsAsync(new System.Exception("SNS Error"));

            var response = await _controller.ReceiveWebhook(validPayload);

            Assert.NotNull(response);
            Assert.Equal(500, response.StatusCode);
            Assert.Equal("{\"error\": \"Internal Server Error\"}", response.Body);
        }

        [Fact]
        public async Task ReceiveWebhook_Should_Extract_ClientIP_From_XForwardedForHeader()
        {
            const string validPayload = "{ \"event\": \"payment\" }";
            const string clientIp = "123.45.67.89";

            Environment.SetEnvironmentVariable("TOPIC_ARN", "arn");
            _httpContext.Request.Headers["X-Forwarded-For"] = clientIp;

            var response = await _controller.ReceiveWebhook(validPayload);

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
        }
    }
}
