using FluentEmailer.Mailtrap;

namespace FluentEmailer.Core.Tests;

[Collection(nameof(SenderTestsFixture))]
public class MailtrapSenderTests : IDisposable
{
    private const string _username = "user@email.com"; // Mailtrap SMTP inbox username
    private const string _password = "Pass.w0rd!"; // Mailtrap SMTP inbox password
    private const int _smtpPort = 5225;

    private readonly SenderTestsFixture _fixture;
    private readonly SimpleSmtpServer _smtpServer;
    private readonly IFluentEmailer _testEmail;

    public MailtrapSenderTests(SenderTestsFixture fixture)
    {
        _fixture = fixture;
        _smtpServer = SimpleSmtpServer.Start(_smtpPort);

        Email.DefaultSender = new MailtrapSender(_username, _password, "localhost", _smtpPort);

        _testEmail = Email
            .From(_fixture.FromEmail, _fixture.FromName)
            .To(_fixture.ToEmail, _fixture.ToName)
            .Subject(fixture.Subject);
    }

    public void Dispose() => _smtpServer.Stop();


    [Fact]
    public void Should_Send_Simple_Email()
    {
        const string messageBody = "<h2>Test</h2>";
        var email = _testEmail
            .Body(messageBody, true);

        var response = email.Send();
        response.ShouldBeSuccessful();

        _smtpServer.ReceivedEmailCount
            .Should().Be(1);
        _smtpServer.ReceivedEmail[0]
            .Should().BeEquivalentTo(new
            {
                MessageParts = new[]
                {
                    new { BodyData = messageBody, HeaderData = "System.Text.ASCIIEncoding+ASCIIEncodingSealed" }
                }
            });
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
            Filename = "mailtrapTest.txt"
        };

        var email = _testEmail
            .Body(_fixture.Body, true)
            .Attach(attachment);

        var response = await email.SendAsync();
        response.ShouldBeSuccessful();

        _smtpServer.ReceivedEmailCount
            .Should().Be(1);
        _smtpServer.ReceivedEmail[0]
            .Should().BeEquivalentTo(new
            {
                MessageParts = new[]
                {
                    new { BodyData = _fixture.Body, HeaderData = "System.Text.ASCIIEncoding+ASCIIEncodingSealed" },
                    new { BodyData = attachmentContents + Environment.NewLine, HeaderData = "text/plain; name=mailtrapTest.txt" }
                }
            });
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

        var email = _testEmail
            .Body(_fixture.Body, true)
            .Attach(attachment);

        var response = await email.SendAsync();
        response.ShouldBeSuccessful();

        _smtpServer.ReceivedEmailCount
            .Should().Be(1);
        _smtpServer.ReceivedEmail[0].MessageParts
            .Should().HaveCount(2);
        _smtpServer.ReceivedEmail[0].MessageParts[0]
            .Should().BeEquivalentTo(new { BodyData = _fixture.Body, HeaderData = "System.Text.ASCIIEncoding+ASCIIEncodingSealed" });
        _smtpServer.ReceivedEmail[0].MessageParts[1].BodyData
            .Should().NotBeNullOrWhiteSpace();
        _smtpServer.ReceivedEmail[0].MessageParts[1].HeaderData
            .Should().Be("image/png; name=logotest.png");
    }
}