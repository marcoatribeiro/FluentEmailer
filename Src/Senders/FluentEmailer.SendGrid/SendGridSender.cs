using FluentEmailer.Core;
using FluentEmailer.Core.Models;
using SendGridAttachment = SendGrid.Helpers.Mail.Attachment;

namespace FluentEmailer.SendGrid;

public sealed class SendGridSender : ISendGridSender
{
    private readonly string _apiKey;
    private readonly SendGridOptions _options = new();

    public SendGridSender(string apiKey, Action<SendGridOptions>? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        options?.Invoke(_options);
        _apiKey = apiKey;
    }

    public SendResponse Send(IFluentEmailer email, CancellationToken token = default) 
        => SendAsync(email, token).GetAwaiter().GetResult();

    public async Task<SendResponse> SendAsync(IFluentEmailer email, CancellationToken token = default)
    {
        var mailMessage = await BuildSendGridMessage(email);

        if (email.Data.IsHtml)
            mailMessage.HtmlContent = email.Data.Body;
        else
            mailMessage.PlainTextContent = email.Data.Body;

        if (!string.IsNullOrEmpty(email.Data.PlaintextAlternativeBody))
            mailMessage.PlainTextContent = email.Data.PlaintextAlternativeBody;

        var sendResponse = await SendViaSendGrid(mailMessage, token);
        return sendResponse;
    }

    public async Task<SendResponse> SendWithTemplateAsync(IFluentEmailer email, string templateId, object templateData, CancellationToken token = default)
    {
        var mailMessage = await BuildSendGridMessage(email);

        mailMessage.SetTemplateId(templateId);
        mailMessage.SetTemplateData(templateData);

        var sendResponse = await SendViaSendGrid(mailMessage, token);
        return sendResponse;
    }

    private async Task<SendGridMessage> BuildSendGridMessage(IFluentEmailer email)
    {
        var mailMessage = new SendGridMessage();
        mailMessage.SetSandBoxMode(_options.SandboxMode);

        mailMessage.SetFrom(ConvertAddress(email.Data.FromAddress));

        if (email.Data.ToAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
            mailMessage.AddTos(email.Data.ToAddresses.Select(ConvertAddress).ToList());

        if (email.Data.CcAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
            mailMessage.AddCcs(email.Data.CcAddresses.Select(ConvertAddress).ToList());

        if (email.Data.BccAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
            mailMessage.AddBccs(email.Data.BccAddresses.Select(ConvertAddress).ToList());

        if (email.Data.ReplyToAddresses.Any(a => !string.IsNullOrWhiteSpace(a.EmailAddress)))
            // SendGrid does not support multiple ReplyTo addresses
            mailMessage.SetReplyTo(email.Data.ReplyToAddresses.Select(ConvertAddress).First());

        mailMessage.SetSubject(email.Data.Subject);

        if (email.Data.Headers.Any())
            mailMessage.AddHeaders(email.Data.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        if (email.Data.Tags.Any())
            mailMessage.Categories = email.Data.Tags.ToList();

        if (email.Data.IsHtml)
            mailMessage.HtmlContent = email.Data.Body;
        else
            mailMessage.PlainTextContent = email.Data.Body;

        switch (email.Data.Priority)
        {
            case Priority.High:
                // https://stackoverflow.com/questions/23230250/set-email-priority-with-sendgrid-api
                mailMessage.AddHeader("Priority", "Urgent");
                mailMessage.AddHeader("Importance", "High");
                // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab
                mailMessage.AddHeader("X-Priority", "1");
                mailMessage.AddHeader("X-MSMail-Priority", "High");
                break;

            case Priority.Normal:
                // Do not set anything.
                // Leave default values. It means Normal Priority.
                break;

            case Priority.Low:
                // https://stackoverflow.com/questions/23230250/set-email-priority-with-sendgrid-api
                mailMessage.AddHeader("Priority", "Non-Urgent");
                mailMessage.AddHeader("Importance", "Low");
                // https://docs.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxcmail/2bb19f1b-b35e-4966-b1cb-1afd044e83ab
                mailMessage.AddHeader("X-Priority", "5");
                mailMessage.AddHeader("X-MSMail-Priority", "Low");
                break;
        }

        if (!email.Data.Attachments.Any()) 
            return mailMessage;

        foreach (var attachment in email.Data.Attachments)
        {
            var sendGridAttachment = await ConvertAttachment(attachment);
            mailMessage.AddAttachment(sendGridAttachment.Filename, sendGridAttachment.Content,
                sendGridAttachment.Type, sendGridAttachment.Disposition, sendGridAttachment.ContentId);
        }

        return mailMessage;
    }

    private async Task<SendResponse> SendViaSendGrid(SendGridMessage mailMessage, CancellationToken token = default)
    {
        var sendGridClient = new SendGridClient(_apiKey, _options.Host, _options.RequestHeaders, _options.Version, _options.UrlPath);
        //var sendGridClient = new SendGridClient(_apiKey);
        var sendGridResponse = await sendGridClient.SendEmailAsync(mailMessage, token);

        var sendResponse = new SendResponse();

        if (sendGridResponse.Headers.TryGetValues("X-Message-ID", out IEnumerable<string>? messageIds))
            sendResponse.MessageId = messageIds.FirstOrDefault() ?? string.Empty;

        if (IsHttpSuccess((int)sendGridResponse.StatusCode)) 
            return sendResponse;

        sendResponse.ErrorMessages.Add($"{sendGridResponse.StatusCode}");
        var messageBodyDictionary = await sendGridResponse.DeserializeResponseBodyAsync();

        if (!messageBodyDictionary.TryGetValue("errors", out var errors)) 
            return sendResponse;

        foreach (var error in errors)
            sendResponse.ErrorMessages.Add($"{error}");

        return sendResponse;
    }

    private static EmailAddress ConvertAddress(Address address) => new(address.EmailAddress, address.Name);

    private static async Task<SendGridAttachment> ConvertAttachment(Core.Models.Attachment attachment) => new()
    {
        Content = await GetAttachmentBase64String(attachment.Data),
        Filename = attachment.Filename,
        Type = attachment.ContentType,
        Disposition = attachment.IsInline ? "inline" : "attachment",
        ContentId = attachment.ContentId
    };

    private static async Task<string> GetAttachmentBase64String(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return Convert.ToBase64String(ms.ToArray());
    }

    private static bool IsHttpSuccess(int statusCode) => statusCode is >= 200 and < 300;
}