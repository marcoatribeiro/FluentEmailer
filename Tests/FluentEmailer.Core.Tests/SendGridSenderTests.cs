using FluentEmailer.SendGrid;

namespace FluentEmailer.Core.Tests;

[Collection(nameof(SenderTestsFixture))]
public class SendGridSenderTests : IDisposable
{
    private const string _apiToken = "SENDGRID_API_TOKEN";
    private readonly SenderTestsFixture _fixture;
    private readonly IFluentEmailer _testEmail;

    public SendGridSenderTests(SenderTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.StartServer();
        Email.DefaultSender = new SendGridSender(_apiToken, opt => opt.Host = _fixture.ServerBaseUrl);

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
    public async Task Should_Send_Email_Using_Template()
    {
        const string templateId = "123456-insert-your-own-id-here";
        object templateData = new
        {
            Name = _fixture.ToEmail,
            ArbitraryValue = "The quick brown fox jumps over the lazy dog."
        };

        var response = await _testEmail
            .SendWithTemplateAsync(templateId, templateData)
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
        await using var stream = File.OpenRead($"{Directory.GetCurrentDirectory()}/test-binary.xlsx");

        var attachment = new Attachment
        {
            Data = stream,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Filename = "test-binary.xlsx"
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
            IsInline = true,
            ContentId = "logotest_id"
        };

        var response = await _testEmail
            .Body(_fixture.Body)
            .Attach(attachment)
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
}