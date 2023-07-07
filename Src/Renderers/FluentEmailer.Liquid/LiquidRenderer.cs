using FluentEmailer.Core.Interfaces;

namespace FluentEmailer.Liquid;

public class LiquidRenderer : ITemplateRenderer
{
    private readonly IOptions<LiquidRendererOptions> _options;
    private readonly LiquidParser _parser;

    public LiquidRenderer(IOptions<LiquidRendererOptions> options)
    {
        _options = options;
        _parser = new LiquidParser();
    }

    public string Parse<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        return ParseAsync(template, model, isHtml).GetAwaiter().GetResult();
    }

    public async Task<string> ParseAsync<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        var rendererOptions = _options.Value;

        // Check for a custom file provider
        var fileProvider = rendererOptions.FileProvider;
        var viewTemplate = ParseTemplate(template);

        var context = new TemplateContext(model, rendererOptions.TemplateOptions)
        {
            // provide some services to all statements
            AmbientValues =
            {
                [AmbientValues.FileProvider] = fileProvider,
                [AmbientValues.Sections] = new Dictionary<string, List<Statement>>()
            },
            Options =
            {
                FileProvider = fileProvider
            }
        };

        rendererOptions.ConfigureTemplateContext?.Invoke(context, model);

        var body = await viewTemplate.RenderAsync(context, rendererOptions.TextEncoder);

        if (!context.AmbientValues.TryGetValue(AmbientValues.Layout, out var layoutPath)) 
            return body;

        // if a layout is specified while rendering a view, execute it
        context.AmbientValues[AmbientValues.Body] = body;
        var layoutTemplate = ParseLiquidFile((string)layoutPath, fileProvider!);

        return await layoutTemplate.RenderAsync(context, rendererOptions.TextEncoder);

    }

    private IFluidTemplate ParseLiquidFile(string path, IFileProvider? fileProvider)
    {
        static void ThrowMissingFileProviderException()
        {
            const string message = "Cannot parse external file, file provider missing";
            throw new ArgumentNullException(nameof(LiquidRendererOptions.FileProvider), message);
        }

        if (fileProvider is null)
            ThrowMissingFileProviderException();

        var fileInfo = fileProvider!.GetFileInfo(path);
        using var stream = fileInfo.CreateReadStream();
        using var sr = new StreamReader(stream);

        return ParseTemplate(sr.ReadToEnd());
    }

    private IFluidTemplate ParseTemplate(string content)
    {
        if (!_parser.TryParse(content, out var template, out var errors))
            throw new Exception(string.Join(Environment.NewLine, errors));

        return template;
    }
}