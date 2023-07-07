namespace FluentEmailer.Core.Models;

public sealed class EmailData
{
    public IList<Address> ToAddresses { get; } = new List<Address>();
    public IList<Address> CcAddresses { get; } = new List<Address>();
    public IList<Address> BccAddresses { get; } = new List<Address>();
    public IList<Address> ReplyToAddresses { get; } = new List<Address>();
    public IList<Attachment> Attachments { get; } = new List<Attachment>();
    public Address FromAddress { get; set; } = new Address();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PlaintextAlternativeBody { get; set; }
    public Priority Priority { get; set; }
    public IList<string> Tags { get; } = new List<string>();

    public bool IsHtml { get; set; }
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
}