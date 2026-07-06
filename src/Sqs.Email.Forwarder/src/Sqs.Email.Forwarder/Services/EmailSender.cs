using Amazon.SimpleEmail;

namespace Sqs.Email.Forwarder.Services;

internal class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    private readonly IAmazonSimpleEmailService _sesClient;

    private readonly Config _config;

    public EmailSender(
        ILogger<EmailSender> logger,
        IAmazonSimpleEmailService sesClient,
        Config config
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sesClient = sesClient ?? throw new ArgumentNullException(nameof(sesClient));
        _config  = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task SendEmailAsync(
        EmailInfo emailInfo,
        string forwardedEmail
        )
    {
        await using var rawMessageStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(forwardedEmail));

        var req = new SendRawEmailRequest
        {
            Source = emailInfo.Resender,
            Destinations = [ _config.EmailRecipient ],
            RawMessage = new RawMessage(rawMessageStream),
        };
        await _sesClient
            .SendRawEmailAsync(req)
            .ConfigureAwait(false);

        _logger.LogInformation("Email sent! Message ID: {EmailInfoMessageId}", emailInfo.MessageId);
    }
}