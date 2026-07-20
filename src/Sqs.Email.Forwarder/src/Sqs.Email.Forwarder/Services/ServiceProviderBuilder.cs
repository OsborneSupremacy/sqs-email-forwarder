using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SimpleEmail;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable MemberCanBePrivate.Global

namespace Sqs.Email.Forwarder.Services;

internal static class ServiceProviderBuilder
{
    public static IServiceProvider Build() =>
        new ServiceCollection()
            .AddConfigFromEnvironmentVariables()
            .AddUtilities()
            .AddVendorServices()
            .AddProviders()
            .AddBusinessServices()
            .BuildServiceProvider();

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddConfigFromEnvironmentVariables()
        {
            var mailBuckets = EnvReader
                .GetStringValue("MAIL_S3_BUCKETS")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if(!mailBuckets.Any())
                throw new InvalidOperationException("No S3 buckets configured in MAIL_S3_BUCKETS environment variable");

            var emailSenders = EnvReader
                .GetStringValue("MAIL_SENDERS")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if(!emailSenders.Any())
               throw new InvalidOperationException("No Senders configured in MAIL_SENDERS environment variable");

            var config = new Config
            {
                MailBuckets = mailBuckets,
                EmailSenders = emailSenders,
                AwsRegion = EnvReader.GetStringValue("AWS_REGION"),
                EmailRecipient = EnvReader.GetStringValue("MAIL_RECIPIENT"),
                StagingBucket = EnvReader.GetStringValue("STAGING_BUCKET")
            };
            return services.AddSingleton(config);
        }

        internal IServiceCollection AddBusinessServices() =>
            services
                .AddSingleton<IEmailSender, EmailSender>()
                .AddSingleton<EmailMimeComposer>()
                .AddSingleton<IEmailTransformer, EmailTransformer>()
                .AddSingleton<IExtractionService, ExtractionService>()
                .AddSingleton<IEmailStager, EmailStager>()
                .AddSingleton<IAggregator, Aggregator>()
                .AddSingleton<IProcessor, Processor>();

        internal IServiceCollection AddProviders() =>
            services
                .AddSingleton<IEmailProvider, EmailProvider>();

        internal IServiceCollection AddUtilities() =>
            services
                .AddLogging(builder => builder.AddLambdaLogger());

        internal IServiceCollection AddVendorServices()
        {
            var region = RegionEndpoint.GetBySystemName(EnvReader.GetStringValue("AWS_REGION"));
            return services
                .AddDefaultAWSOptions(new AWSOptions { Region = region })
                .AddAWSService<IAmazonS3>()
                .AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>(); // AddAWSService fails for SES;
        }
    }
}