namespace FluentEmailer.Core.Tests;

public class SenderTestsFixture
{
    public string ToEmail => "bob@test.com";
    public string ToName => "Bob Tester";
    public string FromEmail => "johno@test.com";
    public string FromName => "John Doe";
    public string Subject => "sup dawg";
    public string Body => "what be the hipitity hap?";
    public string ReplyTo => "reply@email.com";
    public string Tag => "test_Tag";

    public string ToEmail1 => "bob@test.com";
    public string ToEmail2 => "ratface@test.com";

    public string ServerBaseUrl => "http://localhost:9990/";

    private WireMockServer _server = default!;

    public void StartServer(string endPoint = "", string responseBody = "")
    {
        _server = WireMockServer.Start(ServerBaseUrl);
        _server
            .Given(Request.Create()
                .WithPath($"*{endPoint}")
                .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithBody(responseBody)
            );
    }

    public void StopServer() => _server.Stop();
}

[CollectionDefinition(nameof(SenderTestsFixture), DisableParallelization = true)]
public class SenderTestsCollection : ICollectionFixture<SenderTestsFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}