namespace FluentEmailer.MailerSend;

public sealed class Variable
{
    public string Email { get; set; } = default!;
    public IList<Substitution> Substituitions { get; set; } = new List<Substitution>();
}