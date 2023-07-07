namespace FluentEmailer.Core.Interfaces;

public interface ITemplateRenderer
{
    string Parse<T>(string template, T model, bool isHtml = true)
        where T : notnull;
    Task<string> ParseAsync<T>(string template, T model, bool isHtml = true)
        where T : notnull;
}
