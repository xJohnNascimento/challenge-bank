using Moq;
using StarkBank.Domain.Interfaces.Infrastructure;
using StarkBank.Infrastructure.Services;

namespace StarkBank.Infrastructure.Tests.Infrastructure.Services
{
    public class SnsServiceTest
    {
        [Fact]
        public async Task PublishAsync_ReturnsMessageId_WhenPublishSucceeds()
        {
            const string topicArn = "arn:aws:sns:region:account-id:topic";
            const string message = "Test message";
            const string expectedMessageId = "1234567890";

            var mockSnsClient = new Mock<ISnsClient>();
            mockSnsClient
                .Setup(client => client.PublishAsync(topicArn, message))
                .ReturnsAsync(expectedMessageId);

            var snsService = new SnsService(mockSnsClient.Object);

            var result = await snsService.PublishAsync(topicArn, message);

            Assert.Equal(expectedMessageId, result);
        }

        [Fact]
        public async Task PublishAsync_ThrowsException_WhenPublishFails()
        {
            const string topicArn = "arn:aws:sns:region:account-id:topic";
            const string message = "Test message";

            var mockSnsClient = new Mock<ISnsClient>();
            mockSnsClient
                .Setup(client => client.PublishAsync(topicArn, message))
                .ThrowsAsync(new Exception("Publish failed"));

            var snsService = new SnsService(mockSnsClient.Object);

            await Assert.ThrowsAsync<Exception>(() => snsService.PublishAsync(topicArn, message));
        }
    }
}
