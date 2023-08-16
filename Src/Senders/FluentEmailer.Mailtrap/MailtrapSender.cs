namespace FluentEmailer.Mailtrap;

/// <summary>
/// Send emails to a Mailtrap.io inbox
/// </summary>
public sealed class MailtrapSender : ISender, IDisposable
{
    private const int _testPort = 5225;
    private readonly SmtpClient _smtpClient;
    private static readonly int[] _validPorts = { 25, 465, 2525, _testPort };

    /// <summary>
    /// Creates a sender that uses the given Mailtrap credentials, but does not dispose it.
    /// </summary>
    /// <param name="userName">Username of your mailtrap.io SMTP inbox</param>
    /// <param name="password">Password of your mailtrap.io SMTP inbox</param>
    /// <param name="host">Host address for the Mailtrap.io SMTP inbox</param>
    /// <param name="port">Port for the Mailtrap.io SMTP server. Accepted values are 25, 465 or 2525.</param>
    public MailtrapSender(string userName, string password, string? host = "smtp.mailtrap.io", int? port = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Mailtrap UserName needs to be supplied", nameof(userName));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Mailtrap Password needs to be supplied", nameof(password));

        if (port.HasValue && !_validPorts.Contains(port.Value))
            throw new ArgumentException("Mailtrap Port needs to be either 25, 465 or 2525", nameof(port));

        var portValue = port.GetValueOrDefault(2525);

        if (portValue == _testPort)
            _smtpClient = new SmtpClient("localhost", _testPort)
            {
                EnableSsl = false

            };
        else
            _smtpClient = new SmtpClient(host, portValue)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = true
            };
    }

    public void Dispose() => _smtpClient.Dispose();

    public SendResponse Send(IFluentEmailer email, CancellationToken token = default)
    {
        var smtpSender = new SmtpSender(_smtpClient);
        return smtpSender.Send(email, token);
    }

    public Task<SendResponse> SendAsync(IFluentEmailer email, CancellationToken token = default)
    {
        var smtpSender = new SmtpSender(_smtpClient);
        return smtpSender.SendAsync(email, token);
    }
}
