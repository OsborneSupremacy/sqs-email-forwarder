namespace Sqs.Email.Forwarder.Tests;

public class EmailTransformerTests
{
    private readonly IEmailProvider _emailProvider;

    private readonly IEmailTransformer _sut;

    public EmailTransformerTests()
    {
        DotEnv.Load();

        var serviceProvider = ServiceProviderBuilder
            .Build();

        _emailProvider = serviceProvider.GetRequiredService<IEmailProvider>();
        _sut = serviceProvider.GetRequiredService<IEmailTransformer>();
    }

    [Fact]
    public async Task RepackageEmailAsync_GivenValidEmail_ReturnsRepackagedEmail()
    {
        // arrange
        const string messageId = "plain-text-email";
        var receivedEmail = await _emailProvider.GetReceivedEmailAsync(messageId);

        // act
        var result = await _sut.RepackageAndTransformEmailAsync(receivedEmail);

        // assert
        result.MessageId.Should().Be(messageId);
        result.Resender.Should().BeEquivalentTo(receivedEmail.Resender);
        result.MimeEncodedEmail.Should().NotBeNullOrWhiteSpace();
    }
}