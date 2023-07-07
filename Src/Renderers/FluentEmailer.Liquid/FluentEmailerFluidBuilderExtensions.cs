using FluentEmailer.Core.Interfaces;
using FluentEmailer.Liquid;

namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerFluidBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddLiquidRenderer(
        this FluentEmailerServicesBuilder builder,
        Action<LiquidRendererOptions>? configure = null)
    {
        builder.Services.AddOptions<LiquidRendererOptions>();
        if (configure is not null)
            builder.Services.Configure(configure);

        builder.Services.TryAddSingleton<ITemplateRenderer, LiquidRenderer>();
        return builder;
    }
}
