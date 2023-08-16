namespace FluentEmailer.Core.Models;

public sealed class Attachment
{
    /// <summary>
    /// Gets or sets whether the attachment is intended to be used for inline images (changes the parameter name for providers such as MailGun)
    /// </summary>
    public bool IsInline { get; set; }
    public string Filename { get; set; } = string.Empty;
    public Stream Data { get; set; } = default!;
    public string ContentType { get; set; } = string.Empty;
    public string? ContentId { get; set; }
}