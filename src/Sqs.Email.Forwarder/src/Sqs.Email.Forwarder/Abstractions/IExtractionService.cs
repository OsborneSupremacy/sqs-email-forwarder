using MimeKit;

namespace Sqs.Email.Forwarder.Abstractions;

internal interface IExtractionService
{
    public string ExtractSesMessageId(string sqsBody);

    public string ExtractBody(MimeMessage mailObject);

    public MailboxInfo ExtractSenderInfo(InternetAddressList senderList);

    public MailboxInfo ExtractRelevantRecipientInfo(string domain,
        params ImmutableList<InternetAddressList> recipientLists);
}