using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Sqs.Email.Forwarder;

public class Function
{
    private readonly IProcessor _processor;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public Function()
    {
        _processor = ServiceProviderBuilder
            .Build()
            .GetRequiredService<IProcessor>();
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="sqsEvent">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var processedMessageIds =
            await _processor.ProcessMessagesAsync(sqsEvent.Records.ToImmutableList());

        return new SQSBatchResponse
        {
            BatchItemFailures = sqsEvent.Records
                .Where(record => !processedMessageIds.Contains(record.MessageId))
                .Select(record => new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId })
                .ToList()
        };
    }

















}