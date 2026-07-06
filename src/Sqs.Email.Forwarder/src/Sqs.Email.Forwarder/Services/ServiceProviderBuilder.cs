using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SimpleEmail;
using Microsoft.Extensions.DependencyInjection;
using Sqs.Email.Forwarder.Providers;
// ReSharper disable MemberCanBePrivate.Global

namespace Sqs.Email.Forwarder.Services;

internal static class ServiceProviderBuilder
{
    public static IServiceProvider Build() =>
        new ServiceCollection()
            .AddConfig()
            .AddUtilities()
            .AddVendorServices()
            .AddProviders()
            .AddBusinessServices()
            .BuildServiceProvider();

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddConfig()
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
                EmailRecipient = EnvReader.GetStringValue("MAIL_RECIPIENT")
            };
            return services.AddSingleton(config);
        }

        internal IServiceCollection AddBusinessServices() =>
            services
                .AddSingleton<EmailSender>()
                .AddSingleton<EmailTransformer>()
                .AddSingleton<ExtractionService>()
                .AddSingleton<Processor>();

        internal IServiceCollection AddProviders() =>
            services
                .AddSingleton<EmailProvider>();

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