using FluentEmailer.Core;
using FluentEmailer.Core.Extensions;
using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;
using System.Net.Mime;
using Attachment = System.Net.Mail.Attachment;

namespace FluentEmailer.Smtp;

public class SmtpSender : ISender
{
    private readonly Func<SmtpClient>? _clientFactory;
    private readonly SmtpClient? _smtpClient;

    /// <summary>
    /// Creates a sender using the default SMTP settings.
    /// </summary>
    public SmtpSender() 
        : this(() => new SmtpClient()) { }

    /// <summary>
    /// Creates a sender that uses the factory to create and dispose an SmtpClient with each email sent.
    /// </summary>
    public SmtpSender(Func<SmtpClient> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    /// <summary>
    /// Creates a sender that uses the given SmtpClient, but does not dispose it.
    /// </summary>
    public SmtpSender(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public SendResponse Send(IFluentEmailer email, CancellationToken token = default)
    {
        // Uses task.run to negate Synchronisation Context
        // see: https://stackoverflow.com/questions/28333396/smtpclient-sendmailasync-causes-deadlock-when-throwing-a-specific-exception/28445791#28445791
        return Task.Run(() => SendAsync(email, token)).Result;
    }

    public async Task<SendResponse> SendAsync(IFluentEmailer email, CancellationToken token = default)
    {
        var response = new SendResponse();
        var message = CreateMailMessage(email);

        if (token.IsCancellationRequested)
        {
            response.ErrorMessages.Add("Message was cancelled by cancellation token.");
            return response;
        }

        if (_smtpClient is null)
        {
            if (_clientFactory is null)
                throw new InvalidOperationException("Either the client or the client factory must be defined.");
            using var client = _clientFactory();
            await client.SendMailExAsync(message, token);
        }
        else
        {
            await _smtpClient.SendMailExAsync(message, token);
        }

        return response;
    }

    private static MailMessage CreateMailMessage(IFluentEmailer email)
    {
        var data = email.Data;
        MailMessage message;

        // Smtp seems to require the HTML version as the alternative.
        if (!string.IsNullOrEmpty(data.PlaintextAlternativeBody))
        {
            message = new MailMessage
            {
                Subject = data.Subject,
                Body = data.PlaintextAlternativeBody,
                IsBodyHtml = false,
                From = new MailAddress(data.FromAddress.EmailAddress, data.FromAddress.Name)
            };

            var mimeType = new ContentType("text/html; charset=UTF-8");
            AlternateView alternate = AlternateView.CreateAlternateViewFromString(data.Body, mimeType);
            message.AlternateViews.Add(alternate);
        }
        else
        {
            message = new MailMessage
            {
                Subject = data.Subject,
                Body = data.Body,
                IsBodyHtml = data.IsHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                From = new MailAddress(data.FromAddress.EmailAddress, data.FromAddress.Name)
            };
        }

        foreach (var header in data.Headers)
            message.Headers.Add(header.Key, header.Value);

        data.ToAddresses.ForEach(x => message.To.Add(new MailAddress(x.EmailAddress, x.Name)));
        data.CcAddresses.ForEach(x => message.CC.Add(new MailAddress(x.EmailAddress, x.Name)));
        data.BccAddresses.ForEach(x => message.Bcc.Add(new MailAddress(x.EmailAddress, x.Name)));
        data.ReplyToAddresses.ForEach(x => message.ReplyToList.Add(new MailAddress(x.EmailAddress, x.Name)));

        message.Priority = data.Priority switch
        {
            Priority.Low => MailPriority.Low,
            Priority.Normal => MailPriority.Normal,
            Priority.High => MailPriority.High,
            _ => message.Priority
        };

        data.Attachments.ForEach(x =>
        {
            var attachment = new Attachment(x.Data, x.Filename, x.ContentType)
            {
                ContentId = x.ContentId
            };
            if (attachment.ContentDisposition is not null)
                attachment.ContentDisposition.Inline = x.IsInline;
            message.Attachments.Add(attachment);
        });

        return message;
    }
}
