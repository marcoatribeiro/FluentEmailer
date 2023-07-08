using FluentEmailer.Core;
using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;
using FluentEmailer.MailerSend.Utils;

namespace FluentEmailer.MailerSend;

public class MailerSendSender : ISender
{
    private readonly HttpClient _httpClient;
    private readonly MailerSendOptions _options = new();

    public MailerSendSender(string apiKey, Action<MailerSendOptions>? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        options?.Invoke(_options);

        _httpClient = new HttpClient { BaseAddress = new Uri(_options.BaseUrl) };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public SendResponse Send(IFluentEmailer email, CancellationToken token = default)
    {
        return SendAsync(email, token).GetAwaiter().GetResult();
    }

    public async Task<SendResponse> SendAsync(IFluentEmailer email, CancellationToken token = default)
    {
        var emailRequest = EmailRequest.FromEmailData(email.Data, _options);
        var httpResponse = await _httpClient.PostAsJsonAsync(_options.EmailEndPoint, emailRequest, JsonSerializerOptions, token);
        var response = await httpResponse.Content.ReadAsStringAsync(token).ConfigureAwait(false);

        return httpResponse.IsSuccessStatusCode
            ? new SendResponse { MessageId = response }
            : new SendResponse { MessageId = ((int)httpResponse.StatusCode).ToString(), ErrorMessages = new List<string> { response } };
    }

    private static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}