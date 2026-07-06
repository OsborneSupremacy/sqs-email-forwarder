namespace Sqs.Email.Forwarder.Services;

internal class Processor : IProcessor
{
    private readonly ILogger<Processor> _logger;

    private readonly IExtractionService _extractionService;

    private readonly IEmailProvider _emailProvider;

    private readonly IEmailTransformer _emailTransformer;

    private readonly IEmailSender _emailSender;

    public Processor(
        ILogger<Processor> logger,
        IExtractionService extractionService,
        IEmailProvider emailProvider,
        IEmailTransformer emailTransformer,
        IEmailSender emailSender
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

        var receivedEmail = await _emailProvider
            .GetReceivedEmailAsync(messageId)
            .ConfigureAwait(false);

        var repackagedEmail = await _emailTransformer
            .RepackageEmailAsync(receivedEmail)
            .ConfigureAwait(false);

        await _emailSender
            .SendEmailAsync(repackagedEmail)
            .ConfigureAwait(false);;
    }
}