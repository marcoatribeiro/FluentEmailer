namespace FluentEmailer.Core.Tests;

public class SenderTestsFixture
{
    public string ToEmail => "bob@test.com";
    public string FromEmail => "johno@test.com";
    public string Subject => "sup dawg";
    public string Body => "what be the hipitity hap?";
    public string ReplyTo => "reply@email.com";

    public string ToEmail1 => "bob@test.com";
    public string ToEmail2 => "ratface@test.com";
}

[CollectionDefinition(nameof(SenderTestsFixture), DisableParallelization = true)]
public class SenderTestsCollection : ICollectionFixture<SenderTestsFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}