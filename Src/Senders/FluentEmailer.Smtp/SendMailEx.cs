namespace FluentEmailer.Smtp;

// Taken from https://stackoverflow.com/questions/28333396/smtpclient-sendmailasync-causes-deadlock-when-throwing-a-specific-exception/28445791#28445791
// SmtpClient causes deadlock when throwing exceptions. This fixes that.
public static class SendMailEx
{
    public static Task SendMailExAsync(this SmtpClient @this, MailMessage message, CancellationToken token = default)
    {
        // use Task.Run to negate SynchronizationContext
        return Task.Run(() => SendMailExImplAsync(@this, message, token), token);
    }

    private static async Task SendMailExImplAsync(
        SmtpClient client,
        MailMessage message,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<bool>();
        SendCompletedEventHandler handler = default!;
        void Unsubscribe() => client.SendCompleted -= handler;

        async void Handler(object _, AsyncCompletedEventArgs e)
        {
            Unsubscribe();

            // a hack to complete the handler asynchronously
            await Task.Yield();

            if (e.UserState != tcs)
                tcs.TrySetException(new InvalidOperationException("Unexpected UserState"));
            else if (e.Cancelled)
                tcs.TrySetCanceled(token);
            else if (e.Error != null)
                tcs.TrySetException(e.Error);
            else
                tcs.TrySetResult(true);
        }

        handler = Handler;

        client.SendCompleted += handler;
        try
        {
            client.SendAsync(message, tcs);
            await using (token.Register(client.SendAsyncCancel, useSynchronizationContext: false))
            {
                await tcs.Task;
            }
        }
        finally
        {
            Unsubscribe();
        }
    }
}
