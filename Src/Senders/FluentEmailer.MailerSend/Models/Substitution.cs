namespace FluentEmailer.MailerSend;

public sealed class Substitution
{
    public string Var { get; set; } = default!;
    public string Value { get; set; } = default!;
}
