namespace FluentEmailer.MailerSend;

public sealed class MailerSendOptions
{
    public string BaseUrl { get; set; } = "https://api.mailersend.com/v1/";
    public string EmailEndPoint { get; set; } = "email";
    public string? TemplateId { get; set; }
    public bool? PrecedenceBulk { get; set; }
    public IEnumerable<Variable>? Variables { get; set; }
    public IEnumerable<Personalization>? Personalization { get; set; }
    public DateTime? SendAt { get; set; }
}