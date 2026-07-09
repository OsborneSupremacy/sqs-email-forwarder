using MimeKit;

namespace Sqs.Email.Forwarder.Tests;

public class InternetAddressListExtensionsTests
{
    [Fact]
    public void ToDisplayNamesAndEmails_MailboxesContainNamesAndEmails_UsesNameAndEmailFormat()
    {
        // Arrange
        var addresses = new InternetAddressList
        {
            new MailboxAddress("John Doe", "john.doe@example.com"),
            new MailboxAddress("", "jane.doe@example.com")
        };

        // Act
        var result = addresses.ToDisplayNamesAndEmails();

        // Assert
        result.Should().Be("John Doe | john.doe@example.com, jane.doe@example.com");
    }

    [Fact]
    public void ToDisplayNamesAndEmails_EmailOnly_MergedToOneValue()
    {
        // Arrange
        var addresses = new InternetAddressList
        {
            new MailboxAddress("john.doe@example.com", "john.doe@example.com"),
        };

        // Act
        var result = addresses.ToDisplayNamesAndEmails();

        // Assert
        result.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void ToDisplayNamesAndEmails_NameContainsQuotes_RemovesQuotesFromName()
    {
        // Arrange
        var addresses = new InternetAddressList
        {
            new MailboxAddress("\"Jane Doe\"", "jane.doe@example.com")
        };

        // Act
        var result = addresses.ToDisplayNamesAndEmails();

        // Assert
        result.Should().Be("Jane Doe | jane.doe@example.com");
    }

    [Fact]
    public void ToDisplayNamesAndEmails_AddressListIsEmpty_ReturnsEmptyString()
    {
        // Arrange
        var addresses = new InternetAddressList();

        // Act
        var result = addresses.ToDisplayNamesAndEmails();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToDisplayNamesAndEmails_DisplayNamePresentButAddressMissing_UsesUnknownEmailAddress()
    {
        // Arrange
        var addresses = new InternetAddressList
        {
            new GroupAddress("Jane Doe", [])
        };

        // Act
        var result = addresses.ToDisplayNamesAndEmails();

        // Assert
        result.Should().Be("Jane Doe | unknown@unknown.com");
    }
}