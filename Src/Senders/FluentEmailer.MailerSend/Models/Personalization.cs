namespace FluentEmailer.MailerSend;

public sealed class Personalization
{
    public string Email { get; set; } = default!;
    public IList<KeyValuePair<string, object>> Data { get; set; } = new List<KeyValuePair<string, object>>();
}
