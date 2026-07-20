using Amazon.Lambda.SQSEvents;

namespace Sqs.Email.Forwarder.Tests;

public class ProcessorTests
{
    private readonly IProcessor _sut;

    public ProcessorTests()
    {
        DotEnv.Load();

        var serviceProvider = ServiceProviderBuilder
            .Build();

        _sut = serviceProvider.GetRequiredService<IProcessor>();
    }

    [Fact]
    public async Task ProcessMessageAsync_GivenValidMessage_ShouldForwardEmail()
    {
        // arrange
        SQSEvent.SQSMessage htmlEmailMessage = new()
        {
            Body = """
                   {
                     "Type": "Notification",
                     "MessageId": "sns-envelope-id",
                     "TopicArn": "arn:aws:sns:us-east-1:123456789012:ses-topic",
                     "Message": "{\"notificationType\":\"Received\",\"mail\":{\"messageId\":\"html-email\"}}",
                     "Timestamp": "2026-07-06T12:00:00.000Z"
                   }
                   """
        };

        SQSEvent.SQSMessage plainTextEmailMessage = new()
        {
            Body = """
                   {
                     "Type": "Notification",
                     "MessageId": "sns-envelope-id",
                     "TopicArn": "arn:aws:sns:us-east-1:123456789012:ses-topic",
                     "Message": "{\"notificationType\":\"Received\",\"mail\":{\"messageId\":\"plain-text-email\"}}",
                     "Timestamp": "2026-07-06T12:00:00.000Z"
                   }
                   """
        };

        ImmutableList<SQSEvent.SQSMessage> messages = [
            htmlEmailMessage,
            plainTextEmailMessage
        ];

        ImmutableList<string> expectedResult = ["html-email", "plain-text-email"];

        // act
        var processedMessageIds = await _sut.ProcessMessagesAsync(messages);

        // assert
        processedMessageIds.Should().BeEquivalentTo(expectedResult);
    }
}