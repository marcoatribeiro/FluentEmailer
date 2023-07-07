using FluentEmailer.Core.Interfaces;
using FluentEmailer.Smtp;

namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerSmtpBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddSmtpSender(this FluentEmailerServicesBuilder builder, SmtpClient smtpClient)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ISender>(_ => new SmtpSender(smtpClient)));
        return builder;
    }

    public static FluentEmailerServicesBuilder AddSmtpSender(this FluentEmailerServicesBuilder builder, string host, int port) 
        => AddSmtpSender(builder, () => new SmtpClient(host, port));

    public static FluentEmailerServicesBuilder AddSmtpSender(this FluentEmailerServicesBuilder builder, string host, int port, string username, string password)
        => AddSmtpSender(builder,
            () => new SmtpClient(host, port)
                { EnableSsl = true, Credentials = new NetworkCredential(username, password) });

    private static FluentEmailerServicesBuilder AddSmtpSender(this FluentEmailerServicesBuilder builder, Func<SmtpClient> clientFactory)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ISender>(_ => new SmtpSender(clientFactory)));
        return builder;
    }
}