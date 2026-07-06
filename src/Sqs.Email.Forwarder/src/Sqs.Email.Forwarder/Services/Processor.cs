using Sqs.Email.Forwarder.Providers;

namespace Sqs.Email.Forwarder.Services;

internal class Processor
{
    private readonly ILogger<Processor> _logger;

    private readonly ExtractionService _extractionService;

    private readonly EmailProvider _emailProvider;

    private readonly EmailTransformer _emailTransformer;

    private readonly EmailSender _emailSender;

    public Processor(
        ILogger<Processor> logger,
        ExtractionService extractionService,
        EmailProvider emailProvider,
        EmailTransformer emailTransformer,
        EmailSender emailSender
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
        _emailProvider = emailProvider ?? throw new ArgumentNullException(nameof(emailProvider));
        _emailTransformer = emailTransformer ?? throw new ArgumentNullException(nameof(emailTransformer));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task ProcessMessageAsync(SQSEvent.SQSMessage message)
    {
        var messageId = _extractionService.ExtractSesMessageId(message.Body);

        _logger.LogInformation("Processing SES messageId: {MessageId}", messageId);

        var emailInfo = await _emailProvider
            .GetEmailAsync(messageId)
            .ConfigureAwait(false);

        var forwardedEmail = await _emailTransformer
            .TransformToForwardedEmailAsync(emailInfo)
            .ConfigureAwait(false);

        await _emailSender
            .SendEmailAsync(emailInfo, forwardedEmail)
            .ConfigureAwait(false);;
    }
}