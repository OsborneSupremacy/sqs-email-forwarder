using AutoFixture;
using FluentAssertions;
using Sqs.Email.Forwarder.Extensions;
using Xunit;

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