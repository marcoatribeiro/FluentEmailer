using FluentEmailer.Core.Interfaces;
using FluentEmailer.MailerSend;

namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailMailerSendBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddMailerSendSender(
        this FluentEmailerServicesBuilder builder, 
        string apiToken, 
        Action<MailerSendOptions>? options = null)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new MailerSendSender(apiToken, options)));
        return builder;
    }
}