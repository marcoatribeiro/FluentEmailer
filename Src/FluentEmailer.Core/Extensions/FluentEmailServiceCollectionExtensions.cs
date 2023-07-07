using FluentEmailer.Core;
using FluentEmailer.Core.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailServiceCollectionExtensions
{
    public static FluentEmailServicesBuilder AddFluentEmail(this IServiceCollection services, string defaultFromEmail, string defaultFromName = "")
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var builder = new FluentEmailServicesBuilder(services);
        services.TryAdd(ServiceDescriptor.Transient<IFluentEmail>(x => 
            new Email(x.GetRequiredService<ITemplateRenderer>(), x.GetRequiredService<ISender>(), defaultFromEmail, defaultFromName)
        ));

        services.TryAddTransient<IFluentEmailFactory, FluentEmailFactory>();

        return builder;
    }
}

public class FluentEmailServicesBuilder
{
    public IServiceCollection Services { get; private set; }

    internal FluentEmailServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
