using FluentEmailer.Core;
using FluentEmailer.Core.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerServiceCollectionExtensions
{
    public static FluentEmailerServicesBuilder AddFluentEmailer(this IServiceCollection services, string defaultFromEmail, string defaultFromName = "")
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var builder = new FluentEmailerServicesBuilder(services);
        services.TryAdd(ServiceDescriptor.Transient<IFluentEmailer>(x => 
            new Email(x.GetRequiredService<ITemplateRenderer>(), x.GetRequiredService<ISender>(), defaultFromEmail, defaultFromName)
        ));

        services.TryAddTransient<IFluentEmailerFactory, FluentEmailerFactory>();

        return builder;
    }
}

public class FluentEmailerServicesBuilder
{
    public IServiceCollection Services { get; private set; }

    internal FluentEmailerServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
