using FluentEmailer.Core.Interfaces;

namespace FluentEmailer.Razor;

public sealed class RazorRenderer : ITemplateRenderer
{
    private readonly RazorLightEngine _engine;

    public RazorRenderer(string? root = null)
    {
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(root ?? Directory.GetCurrentDirectory())
            .UseMemoryCachingProvider()
            .Build();
    }

    public RazorRenderer(RazorLightProject project)
    {
        _engine = new RazorLightEngineBuilder()
            .UseProject(project)
            .UseMemoryCachingProvider()
            .Build();
    }

    public RazorRenderer(Type embeddedResRootType)
    {
        _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(embeddedResRootType)
            .UseMemoryCachingProvider()
            .Build();
    }

    public string Parse<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        return ParseAsync(template, model, isHtml).GetAwaiter().GetResult();
    }

    public Task<string> ParseAsync<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        dynamic viewBag = (model as IViewBagModel)?.ViewBag!;
        return _engine.CompileRenderStringAsync<T>(GetHashString(template), template, model, viewBag);
    }

    public static string GetHashString(string inputString)
    {
        var sb = new StringBuilder();
        var hashbytes = SHA256.HashData(Encoding.UTF8.GetBytes(inputString));
        foreach (byte b in hashbytes)
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}