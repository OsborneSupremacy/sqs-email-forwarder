namespace Sqs.Email.Forwarder.Abstractions;

/// <summary>
/// Copies a message to the staging S3 bucket with an HTML extension
/// and generates a presigned URL for it.
/// </summary>
internal interface IEmailStager
{
    public Task<StagedEmail> StageAsync(RepackagedEmailInfo repackagedEmailInfo);
}
