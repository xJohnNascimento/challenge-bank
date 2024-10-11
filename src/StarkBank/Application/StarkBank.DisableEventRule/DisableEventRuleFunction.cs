using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StarkBank.DisableEventRule;

public class DisableEventRuleFunction
{
    public async Task FunctionHandler(ILambdaContext context)
    {
        try
        {
            var cloudWatchEventsClient = new AmazonCloudWatchEventsClient();

            var disableRuleRequest3Hours = new DisableRuleRequest
            {
                Name = Environment.GetEnvironmentVariable("EVENT_RULE_NAME")
            };

            var disableThisRuleRequest = new DisableRuleRequest
            {
                Name = Environment.GetEnvironmentVariable("THIS_EVENT_RULE_NAME")
            };

            context.Logger.LogInformation($"Starting DisableRuleAsync {disableRuleRequest3Hours}");
            await cloudWatchEventsClient.DisableRuleAsync(disableRuleRequest3Hours);

            context.Logger.LogInformation($"Starting DisableRuleAsync {disableThisRuleRequest}");
            await cloudWatchEventsClient.DisableRuleAsync(disableThisRuleRequest);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Error:" + ex.Message);
            throw;
        }
    }
}
