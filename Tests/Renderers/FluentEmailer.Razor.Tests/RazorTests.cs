using FluentEmailer.Core;

namespace FluentEmailer.Razor.Tests;

[Collection(nameof(RazorTestsFixture))]
public class RazorTests
{
    private readonly RazorTestsFixture _fixture;

    public RazorTests(RazorTestsFixture fixture) => _fixture = fixture;

    [Fact]
    public void Should_Have_Body_Matching_Template_With_List_Of_Items()
    {
        const string template = "sup @Model.Name here is a list @foreach(var i in Model.Numbers) { @i }";

        var email = CreateEmailerWithDefaultRenderer()
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new { Name = "LUKE", Numbers = new[] { "1", "2", "3" } });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be("sup LUKE here is a list 123");
    }

    [Fact]
    public void Should_Reuse_Cached_Templates()
    {
        const string template1 = "sup @Model.Name here is a list @foreach(var i in Model.Numbers) { @i }";
        const string template2 = "sup @Model.Name this is the second template";

        for (var i = 0; i < 10; i++)
        {
            var email1 = CreateEmailerWithDefaultRenderer()
                .To(_fixture.ToEmail)
                .Subject(_fixture.Subject)
                .UsingTemplate(template1, new { Name = i, Numbers = new[] { "1", "2", "3" } });

            email1.Data.Body
                .Should().NotBeNullOrEmpty()
                .And.Be($"sup {i} here is a list 123");

            var email2 = CreateEmailerWithDefaultRenderer()
                .To(_fixture.ToEmail)
                .Subject(_fixture.Subject)
                .UsingTemplate(template2, new { Name = i });

            email2.Data.Body
                .Should().NotBeNullOrEmpty()
                .And.Be($"sup {i} this is the second template");
        }
    }

    [Fact]
    public void Should_Have_Body_Matching_Template_With_Anonymous_Model()
    {
        const string template = "sup @Model.Name";

        var email = CreateEmailerWithDefaultRenderer()
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new { Name = "LUKE" });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be("sup LUKE");
    }

    [Fact]
    public void Should_Have_Body_Matching_Template_With_Anonymous_Model_With_List()
    {
        const string template = "sup @Model.Name here is a list @foreach(var i in Model.Numbers) { @i }";

        var email = CreateEmailerWithDefaultRenderer()
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new { Name = "LUKE", Numbers = new[] { "1", "2", "3" } });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be("sup LUKE here is a list 123");
    }

    [Fact]
    public void Should_Be_Able_To_Use_Project_Layout_With_Viewbag()
    {
        var projectRoot = Directory.GetCurrentDirectory();
        Email.DefaultRenderer = new RazorRenderer(projectRoot);

        const string template = @"
@{
	Layout = ""./Shared/_Layout.cshtml"";
}
sup @Model.Name here is a list @foreach(var i in Model.Numbers) { @i }";

        dynamic viewBag = new ExpandoObject();
        viewBag.Title = "Hello!";
        var email = new Email(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new ViewModelWithViewBag { Name = "LUKE", Numbers = new[] { "1", "2", "3" }, ViewBag = viewBag });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be($"<h1>Hello!</h1>{Environment.NewLine}<div>{Environment.NewLine}sup LUKE here is a list 123</div>");
    }

    [Fact]
    public void Should_Be_Able_To_Use_Embedded_Layout_With_Viewbag()
    {
        Email.DefaultRenderer = new RazorRenderer(typeof(RazorTests));

        const string template = @"
@{
	Layout = ""_EmbeddedLayout.cshtml"";
}
sup @Model.Name here is a list @foreach(var i in Model.Numbers) { @i }";

        dynamic viewBag = new ExpandoObject();
        viewBag.Title = "Hello!";
        var email = new Email(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new ViewModelWithViewBag { Name = "LUKE", Numbers = new[] { "1", "2", "3" }, ViewBag = viewBag });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be($"<h2>Hello!</h2>{Environment.NewLine}<div>{Environment.NewLine}sup LUKE here is a list 123</div>");
    }

    private IFluentEmailer CreateEmailerWithDefaultRenderer() =>
        new Email(_fixture.FromEmail)
        {
            Renderer = new RazorRenderer()
        };
}