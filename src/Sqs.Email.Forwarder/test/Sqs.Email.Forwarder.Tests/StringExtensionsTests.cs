namespace Sqs.Email.Forwarder.Tests;

public class StringExtensionsTests
{
	[Fact]
	public void GetEmailLocalPart_EmailAddressContainsLocalPart_ReturnsLocalPart()
	{
		// Arrange
		var fixture = new Fixture();
		var sut = fixture
			.Build<string>()
			.FromFactory(() => "john.doe@example.com")
			.OmitAutoProperties()
			.Create();

		// Act
		var result = sut.GetEmailLocalPart();

		// Assert
		result.Should().Be("john.doe");
	}

	[Theory]
	[ClassData(typeof(GetEmailLocalPartWhenInvalidEmailAddressData))]
	public void GetEmailLocalPart_EmailAddressMissingOrInvalidLocalPart_ReturnsUnknown(string emailAddress)
	{
		// Arrange
		var fixture = new Fixture();
		var sut = fixture
			.Build<string>()
			.FromFactory(() => emailAddress)
			.OmitAutoProperties()
			.Create();

		// Act
		var result = sut.GetEmailLocalPart();

		// Assert
		result.Should().Be("unknown");
	}

	[Fact]
	public void GetEmailDomain_EmailAddressContainsDomain_ReturnsDomain()
	{
		// Arrange
		var fixture = new Fixture();
		var sut = fixture
			.Build<string>()
			.FromFactory(() => "john.doe@example.com")
			.OmitAutoProperties()
			.Create();

		// Act
		var result = sut.GetEmailDomain();

		// Assert
		result.Should().Be("example.com");
	}

	[Theory]
	[ClassData(typeof(GetEmailDomainWhenInvalidEmailAddressData))]
	public void GetEmailDomain_EmailAddressMissingOrInvalidDomain_ReturnsUnknownDomain(string emailAddress)
	{
		// Arrange
		var fixture = new Fixture();
		var sut = fixture
			.Build<string>()
			.FromFactory(() => emailAddress)
			.OmitAutoProperties()
			.Create();

		// Act
		var result = sut.GetEmailDomain();

		// Assert
		result.Should().Be("@unknown.com");
	}

	[Fact]
	public void GetEmailDomain_EmailAddressContainsMultipleAtSymbols_ReturnsSubstringAfterFirstAtSymbol()
	{
		// Arrange
		var fixture = new Fixture();
		var sut = fixture
			.Build<string>()
			.FromFactory(() => "john@sub@example.com")
			.OmitAutoProperties()
			.Create();

		// Act
		var result = sut.GetEmailDomain();

		// Assert
		result.Should().Be("sub@example.com");
	}

	[Theory]
	[InlineData("Re: [Alert] Hello, world! #2026", "ReAlertHelloworld2026")]
	[InlineData("SimpleFileName123", "SimpleFileName123")]
	[InlineData("file-name_with.mixed+chars", "filenamewithmixedchars")]
	[InlineData("   spaced   out   ", "spacedout")]
	[InlineData("\tline\nbreak\rtest", "linebreaktest")]
	[InlineData("!!!@@@###", "")]
	[InlineData("", "")]
	[InlineData("abcDEF123", "abcDEF123")]
	[InlineData("0-1-2-3-4-5", "012345")]
	[InlineData("invoice_2026-07-06.pdf", "invoice20260706pdf")]
	[InlineData("john.doe+tag@example.com", "johndoetagexamplecom")]
	[InlineData("[Ticket-42] Server: db-01", "Ticket42Serverdb01")]
	[InlineData("naive cafe resume", "naivecaferesume")]
	[InlineData("Caf\u00e9 d\u00e9j\u00e0 vu", "Cafdjvu")]
	[InlineData("\u65e5\u672c\u8a9e123\u30c6\u30b9\u30c8", "123")]
	[InlineData("emoji \ud83d\ude00 test \ud83d\ude80 99", "emojitest99")]
	[InlineData("A(B)C[D]E{F}<G>", "ABCDEFG")]
	[InlineData("a/b\\c:d*e?f\"g<h>i|j", "abcdefghij")]
	public void ToSafeFileName_StringContainsNonAlphaNumericCharacters_RemovesInvalidCharacters(string input, string expectedOutput)
	{
		// Arrange

		// Act
		var result = input.ToSesAttachmentSafeFileName();

		// Assert
		result.Should().Be(expectedOutput);
	}

	[Fact]
	public void ToSafeFileName_StringExceedsMaxLength_TruncatesToMaxLength()
	{
		// Arrange
		const string sut = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

		// Act
		var result = sut.ToSesAttachmentSafeFileName();

		// Assert
		result.Length.Should().Be(50);
		result.Should().Be("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWX");
	}
}

public class GetEmailLocalPartWhenInvalidEmailAddressData : TheoryData<string>
{
	public GetEmailLocalPartWhenInvalidEmailAddressData()
	{
		Add("@example.com");
		Add("noatsymbol");
	}
}

public class GetEmailDomainWhenInvalidEmailAddressData : TheoryData<string>
{
	public GetEmailDomainWhenInvalidEmailAddressData()
	{
		Add("noatsymbol");
		Add("john@");
	}
}