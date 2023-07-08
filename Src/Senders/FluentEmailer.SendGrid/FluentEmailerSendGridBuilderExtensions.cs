using FluentEmailer.Core.Interfaces;
using FluentEmailer.SendGrid;

namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerSendGridBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddSendGridSender(this FluentEmailerServicesBuilder builder, string apiKey, bool sandBoxMode = false)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ISender>(_ => new SendGridSender(apiKey, opt => opt.SandboxMode = sandBoxMode)));
        return builder;
    }

    public static FluentEmailerServicesBuilder AddSendGridSender(this FluentEmailerServicesBuilder builder, string apiKey, Action<SendGridOptions>? options = null)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ISender>(_ => new SendGridSender(apiKey, options)));
        return builder;
    }
}
