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
            MessageId = "sqs-html-email",
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
            MessageId = "sqs-plain-text-email",
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

        SQSEvent.SQSMessage emailWithAttachment = new()
        {
            MessageId = "sqs-email-with-attachment",
            Body = """
                   {
                     "Type": "Notification",
                     "MessageId": "sns-envelope-id",
                     "TopicArn": "arn:aws:sns:us-east-1:123456789012:ses-topic",
                     "Message": "{\"notificationType\":\"Received\",\"mail\":{\"messageId\":\"email-with-attachment\"}}",
                     "Timestamp": "2026-07-06T12:00:00.000Z"
                   }
                   """
        };


        ImmutableList<SQSEvent.SQSMessage> messages = [
            htmlEmailMessage,
            plainTextEmailMessage,
            emailWithAttachment
        ];

        ImmutableList<string> expectedResult = ["sqs-html-email", "sqs-plain-text-email", "sqs-email-with-attachment"];

        // act
        var processedMessageIds = await _sut.ProcessMessagesAsync(messages);

        // assert
        processedMessageIds.Should().BeEquivalentTo(expectedResult);
    }
}
