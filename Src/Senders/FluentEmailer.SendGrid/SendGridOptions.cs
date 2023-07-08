namespace FluentEmailer.SendGrid;

public sealed class SendGridOptions
{
    public string? UrlPath { get; set; }

    public bool SandboxMode { get; set; }

    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    public string Host { get; set; } = "https://api.sendgrid.com";

    public string Version { get; set; } = "v3";
}