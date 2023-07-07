namespace FluentEmailer.Liquid.Tests;

public class LiquidTestsFixture
{
    public string ToEmail => "bob@test.com";
    public string FromEmail => "johno@test.com";
    public string Subject => "sup dawg";
}

[CollectionDefinition(nameof(LiquidTestsFixture))]
public class LiquidTestsCollection : ICollectionFixture<LiquidTestsFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

internal class ViewModel
{
    public string Name { get; set; } = string.Empty;
    public string[] Numbers { get; set; } = Array.Empty<string>();
}
