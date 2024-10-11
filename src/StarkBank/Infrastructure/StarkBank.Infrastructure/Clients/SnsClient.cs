using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using StarkBank.Domain.Interfaces.Infrastructure;

namespace StarkBank.Infrastructure.Clients
{
    public class SnsClient : ISnsClient
    {
        private readonly IAmazonSimpleNotificationService _snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.SAEast1);

        public async Task<string> PublishAsync(string topicArn, string message)
        {
            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = message
            };

            var response = await _snsClient.PublishAsync(publishRequest);
            return response.MessageId;
        }
    }
}
