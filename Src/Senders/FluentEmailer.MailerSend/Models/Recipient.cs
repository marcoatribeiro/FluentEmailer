namespace FluentEmailer.MailerSend;

public sealed class Recipient
{
    public string? Email { get; set; }
    public string? Name { get; set; }

    [JsonIgnore]
    public Dictionary<string, string>? Substitutions { get; set; }
}
