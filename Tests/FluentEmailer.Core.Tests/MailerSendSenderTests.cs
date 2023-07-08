using FluentEmailer.MailerSend;

namespace FluentEmailer.Core.Tests;

[Collection(nameof(SenderTestsFixture))]
public class MailerSendSenderTests : IDisposable
{
    private const string _apiToken = "MAILSENDER_API_TOKEN";
    private readonly SenderTestsFixture _fixture;

    public MailerSendSenderTests(SenderTestsFixture fixture)
    {
        _fixture = fixture;
        _fixture.StartServer("email");
        Email.DefaultSender = new MailerSendSender(_apiToken, opt => opt.BaseUrl = _fixture.ServerBaseUrl);
    }

    public void Dispose() => _fixture.StopServer();

    [Fact]
    public async Task Should_Send_Email()
    {
        var response = await Email
            .From("sender@email.com", "Test Sender")
            .To("test@email.com", "Test recipient")
            .Subject("FluentEmail MailerSend Test")
            .Body("<html><body><h1>Test</h1><p>Greetings from the team, you got this message through MailerSend.</p></body></html>", true)
            .Tag("test_tag")
            .SendAsync()
            .ConfigureAwait(false);

        response.ShouldBeSuccessful();
    }
}