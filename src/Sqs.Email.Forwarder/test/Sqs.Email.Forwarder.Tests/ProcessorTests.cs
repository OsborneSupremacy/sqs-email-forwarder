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

    [Theory]
    [InlineData("plain-text-email")]
    [InlineData("html-email")]
    public async Task ProcessMessageAsync_GivenValidMessage_ShouldForwardEmail(string messageId)
    {
        // arrange
        SQSEvent.SQSMessage message = new()
        {
            Body = """
                   {
                     "Type": "Notification",
                     "MessageId": "sns-envelope-id",
                     "TopicArn": "arn:aws:sns:us-east-1:123456789012:ses-topic",
                     "Message": "{\"notificationType\":\"Received\",\"mail\":{\"messageId\":\"<MESSAGE-ID>\"}}",
                     "Timestamp": "2026-07-06T12:00:00.000Z"
                   }
                   """.Replace("<MESSAGE-ID>", messageId)
        };

        // act
        var result = async () => {
            await _sut.ProcessMessageAsync(message);
        };

        // asset
        await result.Should().NotThrowAsync();
    }
}