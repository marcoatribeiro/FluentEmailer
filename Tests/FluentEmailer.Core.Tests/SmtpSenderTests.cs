using FluentEmailer.Smtp;
using Attachment = FluentEmailer.Core.Models.Attachment;

namespace FluentEmailer.Core.Tests;

[Collection(nameof(SenderTestsFixture))]
public class SmtpSenderTests : IDisposable
{
    private const int _smtpPort = 5225;

    private readonly SenderTestsFixture _fixture; 
    private readonly SimpleSmtpServer _smtpServer;
    private readonly IFluentEmailer _testEmail;

    public SmtpSenderTests(SenderTestsFixture fixture)
    {
        _fixture = fixture;
        _smtpServer = SimpleSmtpServer.Start(_smtpPort);

        Email.DefaultSender = new SmtpSender(() => new SmtpClient("localhost", _smtpPort)
        {
            EnableSsl = false
        });

        _testEmail = Email
            .From(fixture.FromEmail)
            .To(fixture.ToEmail)
            .Subject(fixture.Subject)
            .Body(fixture.Body);
    }

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
            Filename = "mailgunTest.txt"
        };

        var email = _testEmail
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
                    new { BodyData = attachmentContents + Environment.NewLine, HeaderData = "text/plain; name=mailgunTest.txt" }
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

    [Fact]
    public async Task Should_Send_Body_In_Html_And_Plain_Text_Formats()
    {
        const string htmlBody = "<h2>Test</h2><p>some body text</p>";
        const string plainTextBody = "Test - Some body text";

        var email = _testEmail
            .Body(htmlBody, true)
            .PlaintextAlternativeBody(plainTextBody);

        var response = await email.SendAsync();
        response.ShouldBeSuccessful();

        _smtpServer.ReceivedEmailCount
            .Should().Be(1);
        _smtpServer.ReceivedEmail[0]
            .Should().BeEquivalentTo(new
            {
                MessageParts = new[]
                {
                    new { BodyData = "<h2>Test</h2><p>some body text</p>", HeaderData = "text/html; charset=UTF-8" },
                    new { BodyData = "Test - Some body text", HeaderData = "text/plain; charset=us-ascii" }
                }
            });
    }

    [Fact]
    public void Should_Cancel_If_Cancelation_Is_Requested()
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var response = _testEmail.Send(tokenSource.Token);
        response.ShouldNotBeSuccessful();
    }

    public void Dispose() => _smtpServer.Stop();
}