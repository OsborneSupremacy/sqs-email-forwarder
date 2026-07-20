namespace Sqs.Email.Forwarder.Services;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class Processor : IProcessor
{
    private readonly ILogger<Processor> _logger;

    private readonly IExtractionService _extractionService;

    private readonly IEmailProvider _emailProvider;

    private readonly IEmailTransformer _emailTransformer;

    private readonly IAggregator _aggregator;

    private readonly IEmailStager _emailStager;

    public Processor(
        ILogger<Processor> logger,
        IExtractionService extractionService,
        IEmailProvider emailProvider,
        IEmailTransformer emailTransformer,
        IAggregator aggregator,
        IEmailStager emailStager
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
        _emailProvider = emailProvider ?? throw new ArgumentNullException(nameof(emailProvider));
        _emailTransformer = emailTransformer ?? throw new ArgumentNullException(nameof(emailTransformer));
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        _emailStager = emailStager ?? throw new ArgumentNullException(nameof(emailStager));
    }

    public async Task<ImmutableList<string>> ProcessMessagesAsync(ImmutableList<SQSEvent.SQSMessage> messages)
    {
        List<StagedEmail> stagedEmails = [];
        List<string> processedMessageIds = [];

        foreach (var message in messages)
        {
            try
            {
                var stagedEmail = await ProcessMessageAsync(message);
                stagedEmails.Add(stagedEmail);
                processedMessageIds.Add(message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}: {ErrorMessage}", message.MessageId, ex.Message);
            }
        }

        await _aggregator
            .EmailAggregateAsync(stagedEmails.ToImmutableList())
            .ConfigureAwait(false);

        return processedMessageIds.ToImmutableList();
    }

    private async Task<StagedEmail> ProcessMessageAsync(SQSEvent.SQSMessage message)
    {
        var messageId = _extractionService.ExtractSesMessageId(message.Body);

        _logger.LogInformation("Processing SES messageId: {MessageId}", messageId);

        var receivedEmail = await _emailProvider
            .GetReceivedEmailAsync(messageId)
            .ConfigureAwait(false);

        var repackagedEmail = await _emailTransformer
            .RepackageEmailAsync(receivedEmail)
            .ConfigureAwait(false);

        return await _emailStager
            .StageAsync(repackagedEmail)
            .ConfigureAwait(false);
    }
}
