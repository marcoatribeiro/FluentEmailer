using FluentEmailer.Core.Models;

namespace FluentEmailer.Core.Tests;

[Collection(nameof(SenderTestsFixture))]
public class FluentEmailerTests
{
    private readonly SenderTestsFixture _fixture;

    public FluentEmailerTests(SenderTestsFixture fixture) => _fixture = fixture;

    [Fact]
    public void Should_Have_ToAddress_Set()
    {
        var email = Email
            .From(_fixture.FromEmail)
            .To(_fixture.ToEmail);

        email.Data.ToAddresses
            .Should().NotBeNullOrEmpty()
            .And.HaveCount(1)
            .And.BeEquivalentTo(new[] { new { EmailAddress = _fixture.ToEmail } });
    }

    [Fact]
    public void Should_Have_From_Address_Set()
    {
        var email = Email
            .From(_fixture.FromEmail);

        email.Data.FromAddress
            .Should().NotBeNull()
            .And.BeEquivalentTo(new { EmailAddress = _fixture.FromEmail });
    }

    [Fact]
    public void Should_Have_Subject_Set()
    {
        var email = Email
            .From(_fixture.FromEmail)
            .Subject(_fixture.Subject);

        email.Data.Subject
            .Should().NotBeNull()
            .And.Be(_fixture.Subject);
    }

    [Fact]
    public void Should_Have_Body_Set()
    {
        var email = Email
            .From(_fixture.FromEmail)
            .Body(_fixture.Body);

        email.Data.Body
            .Should().NotBeNull()
            .And.Be(_fixture.Body);
    }

    [Fact]
    public void Should_Have_ReplyTo_Address_Set()
    {
        var email = Email
            .From(_fixture.FromEmail)
            .ReplyTo(_fixture.ReplyTo);

        email.Data.ReplyToAddresses
            .Should().NotBeNull()
            .And.BeEquivalentTo(new [] { new { EmailAddress = _fixture.ReplyTo } });
    }

    [Fact]
    public void Should_Have_Multiple_To_Recipients()
    {
        var email = Email
            .From(_fixture.FromEmail)
            .To(_fixture.ToEmail1)
            .To(_fixture.ToEmail2);

        email.Data.ToAddresses
            .Should().NotBeNullOrEmpty()
            .And.HaveCount(2)
            .And.BeEquivalentTo(new[]
            {
                new { EmailAddress = _fixture.ToEmail1 },
                new { EmailAddress = _fixture.ToEmail2 }
            });
    }

    [Fact]
    public void Can_Add_Multiple_To_Recipients_From_List()
    {
        var emails = new List<Address>
        {
            new(_fixture.ToEmail1),
            new(_fixture.ToEmail2)
        };

        var email = Email
            .From(_fixture.FromEmail)
            .To(emails);

        email.Data.ToAddresses
            .Should().NotBeNullOrEmpty()
            .And.HaveCount(2)
            .And.BeEquivalentTo(new[]
            {
                new { EmailAddress = _fixture.ToEmail1 },
                new { EmailAddress = _fixture.ToEmail2 }
            });
    }

    [Fact]
    public void Can_Add_Multiple_Cc_Recipients_From_List()
    {
        var emails = new List<Address>
        {
            new(_fixture.ToEmail1),
            new(_fixture.ToEmail2)
        };

        var email = Email
            .From(_fixture.FromEmail)
            .CC(emails);

        email.Data.CcAddresses
            .Should().NotBeNullOrEmpty()
            .And.HaveCount(2)
            .And.BeEquivalentTo(new[]
            {
                new { EmailAddress = _fixture.ToEmail1 },
                new { EmailAddress = _fixture.ToEmail2 }
            });
    }

    [Fact]
    public void Can_Add_Multiple_Bcc_Recipients_From_List()
    {
        var emails = new List<Address>
        {
            new(_fixture.ToEmail1),
            new(_fixture.ToEmail2)
        };

        var email = Email
            .From(_fixture.FromEmail)
            .BCC(emails);

        email.Data.BccAddresses
            .Should().NotBeNullOrEmpty()
            .And.HaveCount(2)
            .And.BeEquivalentTo(new[]
            {
                new { EmailAddress = _fixture.ToEmail1 },
                new { EmailAddress = _fixture.ToEmail2 }
            });
    }

    [Fact]
    public void Should_Be_Valid_With_Most_Common_Properties_Set()
    {
        var email = Email
            .From(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .Body(_fixture.Body);

        email.Data
            .Should().NotBeNull()
            .And.BeEquivalentTo(new
            {
                Body = _fixture.Body,
                Subject = _fixture.Subject,
                FromAddress = new { EmailAddress = _fixture.FromEmail },
                ToAddresses = new[] { new { EmailAddress = _fixture.ToEmail } }
            });
    }
}
