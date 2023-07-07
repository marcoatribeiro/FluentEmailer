using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;

namespace FluentEmailer.Core.Defaults;

public sealed class SaveToDiskSender : ISender
{
    private readonly string _directory;

    public SaveToDiskSender(string directory) => _directory = directory;

    public SendResponse Send(IFluentEmailer emailer, CancellationToken token = default)
    {
        return SendAsync(emailer, token).GetAwaiter().GetResult();
    }

    public async Task<SendResponse> SendAsync(IFluentEmailer emailer, CancellationToken token = default)
    {
        var response = new SendResponse();
        await SaveEmailToDisk(emailer);
        return response;
    }

    private async Task<bool> SaveEmailToDisk(IFluentEmailer emailer)
    {
        var random = new Random();
        var filename = Path.Combine(_directory, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{random.Next(1000)}");

        await using var sw = new StreamWriter(File.OpenWrite(filename));
        await sw.WriteLineAsync($"From: {emailer.Data.FromAddress.Name} <{emailer.Data.FromAddress.EmailAddress}>");
        await sw.WriteLineAsync($"To: {string.Join(",", emailer.Data.ToAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
        await sw.WriteLineAsync($"Cc: {string.Join(",", emailer.Data.CcAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
        await sw.WriteLineAsync($"Bcc: {string.Join(",", emailer.Data.BccAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
        await sw.WriteLineAsync($"ReplyTo: {string.Join(",", emailer.Data.ReplyToAddresses.Select(x => $"{x.Name} <{x.EmailAddress}>"))}");
        await sw.WriteLineAsync($"Subject: {emailer.Data.Subject}");
        foreach (var dataHeader in emailer.Data.Headers)
            await sw.WriteLineAsync($"{dataHeader.Key}:{dataHeader.Value}");
        await sw.WriteLineAsync();
        await sw.WriteAsync(emailer.Data.Body);

        return true;
    }
}
