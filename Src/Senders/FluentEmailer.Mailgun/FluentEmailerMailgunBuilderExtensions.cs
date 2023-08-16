namespace Microsoft.Extensions.DependencyInjection;

public static class FluentEmailerMailgunBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddMailGunSender(this FluentEmailerServicesBuilder builder, 
        string domainName, string apiKey, MailGunRegion mailGunRegion = MailGunRegion.USA)
    {
        builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new MailgunSender(domainName, apiKey, mailGunRegion)));
        return builder;
    }
}