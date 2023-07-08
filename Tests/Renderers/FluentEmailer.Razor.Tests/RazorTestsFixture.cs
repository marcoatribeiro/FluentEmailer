namespace FluentEmailer.Razor.Tests;

public sealed class RazorTestsFixture
{
    public string ToEmail => "bob@test.com";
    public string FromEmail => "johno@test.com";
    public string Subject => "sup dawg";
}

[CollectionDefinition(nameof(RazorTestsFixture))]
public class RazorTestsCollection : ICollectionFixture<RazorTestsFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public class ViewModelWithViewBag : IViewBagModel
{
    public ExpandoObject ViewBag { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public string[] Numbers { get; set; } = Array.Empty<string>();
}
