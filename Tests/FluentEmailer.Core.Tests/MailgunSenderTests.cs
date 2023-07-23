using FluentEmailer.Mailgun;
using Attachment = FluentEmailer.Core.Models.Attachment;

namespace FluentEmailer.Core.Tests;

[Collection(nameof(SenderTestsFixture))]
public class MailgunSenderTests : IDisposable
{
    private readonly SenderTestsFixture _fixture;
    private readonly IFluentEmailer _testEmail;
    private const string _domainName = "MAILGUN_DOMAIN_NAME";
    private const string _apiToken = "SENDGRID_API_TOKEN";

    public MailgunSenderTests(SenderTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.StartServer(_domainName + "/messages", @"{ ""Data"" : { ""Id"" : ""1"" } }");
        Email.DefaultSender = new MailgunSender(_domainName, _apiToken, MailGunRegion.TESTS, _fixture.ServerBaseUrl);

        _testEmail = Email
            .From(_fixture.FromEmail, _fixture.FromName)
            .To(_fixture.ToEmail, _fixture.ToName)
            .Subject(fixture.Subject);
    }

    public void Dispose() => _fixture.StopServer();

    [Fact]
    public async Task Should_Send_Email_With_Tag()
    {
        var response = await _testEmail
            .Body("<html><body><h1>Test</h1><p>Greetings from the team, you got this message through SendGrid.</p></body></html>", true)
            .Tag(_fixture.Tag)
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    [Fact]
    public async Task Should_Send_Email_With_ReplyTo()
    {
        var response = await _testEmail
            .ReplyTo(_fixture.ToEmail, _fixture.ToName)
            .Body(_fixture.Body)
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    [Fact]
    public async Task Should_Send_Email_With_Attachments()
    {
        const string attachmentContents = "Hey this is some text in an attachment";
        using var stream = new MemoryStream();
        await using var sw = new StreamWriter(stream);
        await sw.WriteLineAsync(attachmentContents);
        await sw.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);

        var attachment = new Attachment
        {
            Data = stream,
            ContentType = "text/plain",
            Filename = "mailgunTest.txt"
        };

        var response = await _testEmail
            .Body(_fixture.Body)
            .Attach(attachment)
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    [Fact]
    public async Task Should_Send_Email_With_Inline_Attachments()
    {
        await using var stream = File.OpenRead($"{Directory.GetCurrentDirectory()}/logotest.png");

        var attachment = new Attachment
        {
            Data = stream,
            ContentType = "image/png",
            Filename = "logotest.png",
            IsInline = true
        };

        var response = await _testEmail
            .Body("<html>Inline image here: <img src=\"cid:logotest.png\">" +
                  "<p>You should see an image without an attachment, or without a download prompt, depending on the email client.</p></html>", true)
            .Attach(attachment)
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    [Fact]
    public async Task Should_Send_Email_With_Variables()
    {
        var response = await _testEmail
            .Body(_fixture.Body)
            .Header("X-Mailgun-Variables", JsonSerializer.Serialize(new Variable { Var1 = "Test" }))
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    [Fact]
    public async Task Should_Send_High_Priority_Email()
    {
        var response = await _testEmail
            .Body(_fixture.Body)
            .HighPriority()
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    [Fact]
    public async Task Should_Send_Low_Priority_Email()
    {
        var response = await _testEmail
            .Body(_fixture.Body)
            .LowPriority()
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }

    private class Variable
    {
        public string Var1 { get; set; } = string.Empty;
    }
}
