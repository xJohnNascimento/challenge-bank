using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace StarkBank.DisableEventRule;

public class DisableEventRuleFunction
{
    public async Task FunctionHandler(string input, ILambdaContext context)
    {
        var cloudWatchEventsClient = new AmazonCloudWatchEventsClient();
        var disableRuleRequest = new DisableRuleRequest
        {
            Name = Environment.GetEnvironmentVariable("EVENT_RULE_NAME")
        };

        await cloudWatchEventsClient.DisableRuleAsync(disableRuleRequest);
    }
}
