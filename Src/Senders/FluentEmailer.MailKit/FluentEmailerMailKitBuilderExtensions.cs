namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerMailKitBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddMailKitSender(this FluentEmailerServicesBuilder builder, SmtpClientOptions smtpClientOptions)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new MailKitSender(smtpClientOptions)));
        return builder;
    }
}
