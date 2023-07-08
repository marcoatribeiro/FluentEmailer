﻿using FluentEmailer.Core.Interfaces;

namespace FluentEmailer.Razor;

public static class FluentEmailerRazorBuilderExtensions
{
    public static FluentEmailerServicesBuilder AddRazorRenderer(this FluentEmailerServicesBuilder builder)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ITemplateRenderer, RazorRenderer>(_ => new RazorRenderer()));
        return builder;
    }

    /// <summary>
    /// Add razor renderer with project views and layouts
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="templateRootFolder"></param>
    /// <returns></returns>
    public static FluentEmailerServicesBuilder AddRazorRenderer(this FluentEmailerServicesBuilder builder, string templateRootFolder)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ITemplateRenderer, RazorRenderer>(_ => new RazorRenderer(templateRootFolder)));
        return builder;
    }

    /// <summary>
    /// Add razor renderer with embedded views and layouts
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="embeddedResourceRootType"></param>
    /// <returns></returns>
    public static FluentEmailerServicesBuilder AddRazorRenderer(this FluentEmailerServicesBuilder builder, Type embeddedResourceRootType)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ITemplateRenderer, RazorRenderer>(_ => new RazorRenderer(embeddedResourceRootType)));
        return builder;
    }

    /// <summary>
    /// Add razor renderer with a RazorLightProject to support views and layouts
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="razorLightProject"></param>
    /// <returns></returns>
    public static FluentEmailerServicesBuilder AddRazorRenderer(this FluentEmailerServicesBuilder builder, RazorLightProject razorLightProject)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ITemplateRenderer, RazorRenderer>(_ => new RazorRenderer(razorLightProject)));
        return builder;
    }
}