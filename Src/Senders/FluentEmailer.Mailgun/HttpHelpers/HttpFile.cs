namespace FluentEmailer.Mailgun.HttpHelpers;

public class HttpFile
{
    public string ParameterName { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public Stream Data { get; set; } = default!;
    public string ContentType { get; set; } = string.Empty;
}