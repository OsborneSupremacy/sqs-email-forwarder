using Amazon.S3;
using Amazon.S3.Model;

namespace Sqs.Email.Forwarder.Providers;

internal class EmailProvider : IEmailProvider
{
    private readonly ILogger<EmailProvider> _logger;

    private readonly Config _config;

    private readonly IAmazonS3 _s3Client;

    public EmailProvider(
        ILogger<EmailProvider> logger,
        Config config,
        IAmazonS3 s3Client
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
    }

    public async Task<ReceivedEmailInfo> GetReceivedEmailAsync(string messageId)
    {
        var bucketIndex = await GetObjectBucketIndexAsync(messageId);
        var bucket = _config.MailBuckets[bucketIndex];

        var obj = await _s3Client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = bucket,
            Key = messageId
        });

        await using var ms = new MemoryStream();
        await obj.ResponseStream.CopyToAsync(ms);
        var rawEmail = ms.ToArray();

        var emailSender = _config.EmailSenders[bucketIndex];
        var downloadFileName = messageId.ToSesAttachmentSafeFileName();
        if (string.IsNullOrWhiteSpace(downloadFileName))
            downloadFileName = "email";

        var presignedUrl = await _s3Client
            .GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = messageId,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddDays(7),
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentType = "message/rfc822",
                    ContentDisposition = $"attachment; filename=\"{downloadFileName}.eml\""
                }
            })
            .ConfigureAwait(false);

        return new ReceivedEmailInfo
        {
            MessageId = messageId,
            Resender = _config.EmailSenders[bucketIndex],
            RawEmail = rawEmail,
            Domain = emailSender.GetEmailDomain(),
            Url = presignedUrl
        };
    }

    private async Task<int> GetObjectBucketIndexAsync(string messageId)
    {
        var bucketIndex = 0;
        foreach (var bucket in _config.MailBuckets)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = bucket,
                    Key = messageId
                });
                return bucketIndex;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                bucketIndex++;
            }
        }

        throw new InvalidRequestException("Email not found in any configured bucket");
    }
}
