using Amazon.S3;
using Amazon.S3.Model;

namespace Sqs.Email.Forwarder.Services;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class EmailStager : IEmailStager
{
    private readonly ILogger<EmailStager> _logger;

    private readonly Config _config;

    private readonly IAmazonS3 _s3Client;

    public EmailStager(ILogger<EmailStager> logger, Config config, IAmazonS3 s3Client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
    }

    public async Task<StagedEmail> StageAsync(RepackagedEmailInfo repackagedEmailInfo)
    {
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);

        var senderEmail = Uri.EscapeDataString(repackagedEmailInfo.OriginalSenderEmail);
        var objectKey = $"{localTime:MM/dd/}{senderEmail}/{localTime:HH-mm-}{repackagedEmailInfo.MessageId}.html";

        var putRequest = new PutObjectRequest
        {
            BucketName = _config.StagingBucket,
            Key = objectKey,
            ContentBody = repackagedEmailInfo.HtmlBody,
            ContentType = "text/html; charset=utf-8"
        };

        await _s3Client
            .PutObjectAsync(putRequest)
            .ConfigureAwait(false);

        var presignedUrl = await _s3Client
            .GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = _config.StagingBucket,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddDays(7)
            })
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Staged email {MessageId} in S3 bucket {BucketName} with key {ObjectKey}",
            repackagedEmailInfo.MessageId,
            _config.StagingBucket,
            objectKey);

        return new StagedEmail
        {
            MessageId = repackagedEmailInfo.MessageId,
            OriginalSubject = repackagedEmailInfo.SubjectOriginal,
            OriginalSenderEmail = repackagedEmailInfo.OriginalSenderEmail,
            OriginalRecipientEmail = repackagedEmailInfo.OriginalRecipientEmail,
            HasAttachments = repackagedEmailInfo.HasAttachments,
            OriginalDate = repackagedEmailInfo.OriginalDate,
            Domain = repackagedEmailInfo.OriginalSenderEmail.GetEmailDomain(),
            OriginalUrl = repackagedEmailInfo.OriginalUrl,
            PresignedUrl = presignedUrl
        };
    }
}
