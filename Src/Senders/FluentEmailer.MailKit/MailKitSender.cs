namespace FluentEmailer.MailKit;

/// <summary>
/// Send emails with the MailKit Library.
/// </summary>
public sealed class MailKitSender : ISender
{
    private readonly SmtpClientOptions _smtpClientOptions;

    /// <summary>
    /// Creates a sender that uses the given SmtpClientOptions when sending with MailKit. Since the client is internal this will dispose of the client.
    /// </summary>
    /// <param name="smtpClientOptions">The SmtpClientOptions to use to create the MailKit client</param>
    public MailKitSender(SmtpClientOptions smtpClientOptions)
    {
        _smtpClientOptions = smtpClientOptions;
    }

    /// <summary>
    /// Send the specified email.
    /// </summary>
    /// <returns>A response with any errors and a success boolean.</returns>
    /// <param name="email">Email.</param>
    /// <param name="token">Cancellation Token.</param>
    public SendResponse Send(IFluentEmailer email, CancellationToken token = default)
    {
        var response = new SendResponse();
        var message = CreateMailMessage(email);

        if (token.IsCancellationRequested)
        {
            response.ErrorMessages.Add("Message was cancelled by cancellation token.");
            return response;
        }

        try
        {
            if (_smtpClientOptions.UsePickupDirectory)
            {
                SaveToPickupDirectory(message, _smtpClientOptions.MailPickupDirectory).Wait(token);
                return response;
            }

            using var client = new SmtpClient();
            if (_smtpClientOptions.SocketOptions.HasValue)
            {
                client.Connect(
                    _smtpClientOptions.Server,
                    _smtpClientOptions.Port,
                    _smtpClientOptions.SocketOptions.Value,
                    token);
            }
            else
            {
                client.Connect(
                    _smtpClientOptions.Server,
                    _smtpClientOptions.Port,
                    _smtpClientOptions.UseSsl,
                    token);
            }

            // Note: only needed if the SMTP server requires authentication
            if (_smtpClientOptions.RequiresAuthentication)
                client.Authenticate(_smtpClientOptions.User, _smtpClientOptions.Password, token);

            client.Send(message, token);
            client.Disconnect(true, token);
        }
        catch (Exception ex)
        {
            response.ErrorMessages.Add(ex.Message);
        }

        return response;
    }

    /// <summary>
    /// Send the specified email.
    /// </summary>
    /// <returns>A response with any errors and a success boolean.</returns>
    /// <param name="email">Email.</param>
    /// <param name="token">Cancellation Token.</param>
    public async Task<SendResponse> SendAsync(IFluentEmailer email, CancellationToken token = default)
    {
        var response = new SendResponse();
        var message = CreateMailMessage(email);

        if (token.IsCancellationRequested)
        {
            response.ErrorMessages.Add("Message was cancelled by cancellation token.");
            return response;
        }

        try
        {
            if (_smtpClientOptions.UsePickupDirectory)
            {
                await SaveToPickupDirectory(message, _smtpClientOptions.MailPickupDirectory);
                return response;
            }

            using var client = new SmtpClient();
            if (_smtpClientOptions.SocketOptions.HasValue)
            {
                await client.ConnectAsync(
                    _smtpClientOptions.Server,
                    _smtpClientOptions.Port,
                    _smtpClientOptions.SocketOptions.Value,
                    token);
            }
            else
            {
                await client.ConnectAsync(
                    _smtpClientOptions.Server,
                    _smtpClientOptions.Port,
                    _smtpClientOptions.UseSsl,
                    token);
            }

            // Note: only needed if the SMTP server requires authentication
            if (_smtpClientOptions.RequiresAuthentication)
                await client.AuthenticateAsync(_smtpClientOptions.User, _smtpClientOptions.Password, token);

            await client.SendAsync(message, token);
            await client.DisconnectAsync(true, token);
        }
        catch (Exception ex)
        {
            response.ErrorMessages.Add(ex.Message);
        }

        return response;
    }

    /// <summary>
    /// Saves email to a pickup directory.
    /// </summary>
    /// <param name="message">Message to save for pickup.</param>
    /// <param name="pickupDirectory">Pickup directory.</param>
    private static async Task SaveToPickupDirectory(MimeMessage message, string pickupDirectory)
    {
        // Note: this will require that you know where the specified pickup directory is.
        var path = Path.Combine(pickupDirectory, Guid.NewGuid() + ".eml");

        if (File.Exists(path))
            return;

        try
        {
            await using var stream = new FileStream(path, FileMode.CreateNew);
            await message.WriteToAsync(stream);
        }
        catch (IOException)
        {
            // The file may have been created between our File.Exists() check and
            // our attempt to create the stream.
            throw;
        }
    }

    /// <summary>
    /// Create a MimMessage so MailKit can send it
    /// </summary>
    /// <returns>The mail message.</returns>
    /// <param name="email">Email data.</param>
    private static MimeMessage CreateMailMessage(IFluentEmailer email)
    {
        var data = email.Data;

        var message = new MimeMessage();

        // if for any reason, subject header is not added, add it else update it.
        if (!message.Headers.Contains(HeaderId.Subject))
            message.Headers.Add(HeaderId.Subject, Encoding.UTF8, data.Subject ?? string.Empty);
        else
            message.Headers[HeaderId.Subject] = data.Subject ?? string.Empty;

        message.Headers.Add(HeaderId.Encoding, Encoding.UTF8.EncodingName);

        message.From.Add(new MailboxAddress(data.FromAddress.Name, data.FromAddress.EmailAddress));

        var builder = new BodyBuilder();
        if (!string.IsNullOrEmpty(data.PlaintextAlternativeBody))
        {
            builder.TextBody = data.PlaintextAlternativeBody;
            builder.HtmlBody = data.Body;
        }
        else if (!data.IsHtml)
        {
            builder.TextBody = data.Body;
        }
        else
        {
            builder.HtmlBody = data.Body;
        }

        data.Attachments.ForEach(x =>
        {
            var attachment = builder.Attachments.Add(x.Filename, x.Data, ContentType.Parse(x.ContentType));
            attachment.ContentId = x.ContentId;
        });

        message.Body = builder.ToMessageBody();

        foreach (var header in data.Headers)
            message.Headers.Add(header.Key, header.Value);

        data.ToAddresses.ForEach(x => message.To.Add(new MailboxAddress(x.Name, x.EmailAddress)));
        data.CcAddresses.ForEach(x => message.Cc.Add(new MailboxAddress(x.Name, x.EmailAddress)));
        data.BccAddresses.ForEach(x => message.Bcc.Add(new MailboxAddress(x.Name, x.EmailAddress)));
        data.ReplyToAddresses.ForEach(x => message.ReplyTo.Add(new MailboxAddress(x.Name, x.EmailAddress)));

        message.Priority = data.Priority switch
        {
            Priority.Low => MessagePriority.NonUrgent,
            Priority.Normal => MessagePriority.Normal,
            Priority.High => MessagePriority.Urgent,
            _ => message.Priority
        };

        return message;
    }
}
