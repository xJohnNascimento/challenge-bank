using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using StarkBank.Core.Infrastructure;

namespace StarkBank.Infrastructure.Services
{
    public class SnsService : ISnsService
    {
        private readonly IAmazonSimpleNotificationService _snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.SAEast1);

        public async Task<string> PublishAsync(string topicArn, string message)
        {
            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = message
            };

            try
            {
                var response = await _snsClient.PublishAsync(publishRequest);
                return response.MessageId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
                throw;
            }
        }
    }
}
