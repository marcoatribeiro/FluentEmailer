using Microsoft.Extensions.DependencyInjection;

namespace FluentEmailer.Core;

public class FluentEmailerFactory : IFluentEmailerFactory
{
    private readonly IServiceProvider _services;

    public FluentEmailerFactory(IServiceProvider services) => _services = services;

    public IFluentEmailer Create() => _services.GetRequiredService<IFluentEmailer>();
}
