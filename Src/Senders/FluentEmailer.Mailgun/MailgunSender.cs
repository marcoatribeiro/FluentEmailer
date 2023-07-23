namespace FluentEmailer.Mailgun;

public class MailgunSender : ISender
{
    private readonly HttpClient _httpClient;

    public MailgunSender(string domainName, string apiKey, MailGunRegion mailGunRegion = MailGunRegion.USA, string? testsBaseUrl = null)
    {
        var url = mailGunRegion switch
        {
            MailGunRegion.USA => $"https://api.mailgun.net/v3/{domainName}/",
            MailGunRegion.EU => $"https://api.eu.mailgun.net/v3/{domainName}/",
            MailGunRegion.TESTS => $"{testsBaseUrl}{domainName}/",
            _ => throw new ArgumentException($"'{mailGunRegion}' is not a valid value for {nameof(mailGunRegion)}")
        };
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(url)
        };

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}")));
    }

    public SendResponse Send(IFluentEmailer email, CancellationToken token = default)
    {
        return SendAsync(email, token).GetAwaiter().GetResult();
    }

    public async Task<SendResponse> SendAsync(IFluentEmailer email, CancellationToken token = default)
    {
        var parameters = new List<KeyValuePair<string, string>>();

        parameters.Add(new KeyValuePair<string, string>("from", $"{email.Data.FromAddress.Name} <{email.Data.FromAddress.EmailAddress}>"));
        email.Data.ToAddresses.ForEach(x => {
            parameters.Add(new KeyValuePair<string, string>("to", $"{x.Name} <{x.EmailAddress}>"));
        });
        email.Data.CcAddresses.ForEach(x => {
            parameters.Add(new KeyValuePair<string, string>("cc", $"{x.Name} <{x.EmailAddress}>"));
        });
        email.Data.BccAddresses.ForEach(x => {
            parameters.Add(new KeyValuePair<string, string>("bcc", $"{x.Name} <{x.EmailAddress}>"));
        });
        email.Data.ReplyToAddresses.ForEach(x => {
            parameters.Add(new KeyValuePair<string, string>("h:Reply-To", $"{x.Name} <{x.EmailAddress}>"));
        });
        parameters.Add(new KeyValuePair<string, string>("subject", email.Data.Subject));

        parameters.Add(new KeyValuePair<string, string>(email.Data.IsHtml ? "html" : "text", email.Data.Body));

        if (!string.IsNullOrEmpty(email.Data.PlaintextAlternativeBody))
            parameters.Add(new KeyValuePair<string, string>("text", email.Data.PlaintextAlternativeBody));

        email.Data.Tags.ForEach(x =>
        {
            parameters.Add(new KeyValuePair<string, string>("o:tag", x));
        });

        foreach (var emailHeader in email.Data.Headers)
        {
            var key = emailHeader.Key;
            if (!key.StartsWith("h:"))
                key = "h:" + emailHeader.Key;

            parameters.Add(new KeyValuePair<string, string>(key, emailHeader.Value));
        }

        var files = new List<HttpFile>();
        email.Data.Attachments.ForEach(x =>
        {
            var param = x.IsInline ? "inline" : "attachment";

            files.Add(new HttpFile
            {
                ParameterName = param,
                Data = x.Data,
                Filename = x.Filename,
                ContentType = x.ContentType
            });
        });

        var response = await _httpClient
            .PostMultipart<MailgunResponse>("messages", parameters, files)
            .ConfigureAwait(false);

        var result = new SendResponse { MessageId = response.Data.Id };
        if (!response.Success)
            result.ErrorMessages.AddRange(response.Errors.Select(x => x.ErrorMessage));

        return result;
    }
}