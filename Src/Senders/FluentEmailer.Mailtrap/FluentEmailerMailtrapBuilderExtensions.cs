namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerMailtrapBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddMailtrapSender(this FluentEmailerServicesBuilder builder, 
        string userName, string password, string? host = null, int? port = null)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new MailtrapSender(userName, password, host, port)));
        return builder;
    }
}
