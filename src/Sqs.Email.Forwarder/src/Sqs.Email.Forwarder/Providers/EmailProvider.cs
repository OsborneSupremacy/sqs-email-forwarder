using Amazon.S3;
using Amazon.S3.Model;
using Sqs.Email.Forwarder.Models;

namespace Sqs.Email.Forwarder.Providers;

internal class EmailProvider
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

    public async Task<EmailInfo> GetEmailAsync(string messageId)
    {
        var bucketIndex = 0;
        foreach(var bucket in _config.MailBuckets)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = bucket,
                    Key = messageId
                });
            } catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Email with messageId {MessageId} not found in bucket {Bucket}, exception: {ExMessage}", messageId, bucket, ex.Message);
                bucketIndex++;
                continue;
            }

            var obj = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = messageId
            });

            await using var ms = new MemoryStream();
            await obj.ResponseStream.CopyToAsync(ms);
            var rawEmail = ms.ToArray();

            var emailSender = _config.EmailSenders[bucketIndex];
            var emailDomain = GetEmailDomain(emailSender);

            return new EmailInfo
            {
                MessageId = messageId,
                Resender = _config.EmailSenders[bucketIndex],
                RawEmail = rawEmail,
                Domain = emailDomain,
                Url = $"https://s3.console.aws.amazon.com/s3/object/{bucket}/{messageId}?region={_config.AwsRegion}"
            };
        }
        throw new InvalidRequestException("Email not found in any configured bucket");
    }

    private static string GetEmailDomain(string emailAddress)
    {
        var atIndex = emailAddress.IndexOf('@');
        if (atIndex < 0 || atIndex == emailAddress.Length - 1)
            return "@unknown.com";
        return emailAddress[(atIndex + 1)..];
    }
}