using Amazon.S3.Model;

namespace Sqs.Email.Forwarder.Tests;

public class EmailProviderTests
{
    private readonly IEmailProvider _sut;

    public EmailProviderTests()
    {
        DotEnv.Load();

        var serviceProvider = ServiceProviderBuilder
            .Build();

        _sut = serviceProvider.GetRequiredService<IEmailProvider>();
    }

    [Fact]
    public async Task GetReceivedEmailAsync_GivenNonExistentMessage_ThrowsException()
    {
        // arrange
        const string messageId = "does-not-exist";

        // act
        var result = async () => {
            await _sut.GetReceivedEmailAsync(messageId);
        };

        // assert
        await result.Should().ThrowAsync<InvalidRequestException>();
    }

    [Fact]
    public async Task GetReceivedEmailAsync_GivenExistingMessage_ReturnsReceivedEmail()
    {
        // arrange
        const string messageId = "plain-text-email";

        // act
        var result = await _sut.GetReceivedEmailAsync(messageId);

        // assert
        result.RawEmail.Should().NotBeNullOrEmpty();
    }
}