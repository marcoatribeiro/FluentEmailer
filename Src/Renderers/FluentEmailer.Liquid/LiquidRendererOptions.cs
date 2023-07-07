namespace FluentEmailer.Liquid;

public class LiquidRendererOptions
{
    /// <summary>
    /// Allows configuring template context before running the template. Parameters are context that has been
    /// prepared and the model that is going to be used.
    /// </summary>
    /// <remarks>
    /// This API creates dependency on Fluid.
    /// </remarks>
    public Action<TemplateContext, object>? ConfigureTemplateContext { get; init; }

    /// <summary>
    /// Text encoder to use. Defaults to <see cref="HtmlEncoder"/>.
    /// </summary>
    public TextEncoder TextEncoder { get; set; } = HtmlEncoder.Default;

    /// <summary>
    /// File provider to use, used when resolving references in templates, like master layout.
    /// </summary>
    public IFileProvider? FileProvider { get; init; }

    /// <summary>
    /// Set custom Template Options for Fluid 
    /// </summary>
    public TemplateOptions TemplateOptions { get; set; } = new();
}