using FluentEmailer.Core.Interfaces;

namespace FluentEmailer.Core.Defaults;

public class ReplaceRenderer : ITemplateRenderer
{
    public string Parse<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        return model.GetType().GetRuntimeProperties()
            .Aggregate(template, (current, pi) => current
                .Replace($"##{pi.Name}##", pi.GetValue(model, null)?.ToString()));
    }

    public Task<string> ParseAsync<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        return Task.FromResult(Parse(template, model, isHtml));
    }
}
