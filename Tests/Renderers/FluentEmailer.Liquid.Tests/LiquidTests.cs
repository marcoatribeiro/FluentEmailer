namespace FluentEmailer.Liquid.Tests;

[Collection(nameof(LiquidTestsFixture))]
public class LiquidTests
{
    private readonly LiquidTestsFixture _fixture;

    public LiquidTests(LiquidTestsFixture fixture)
    {
        _fixture = fixture;
        // default to have no file provider, only required when layout files are in use
        SetupRenderer();
    }

    private static void SetupRenderer(
        IFileProvider? fileProvider = null,
        Action<TemplateContext, object>? configureTemplateContext = null)
    {
        var options = new LiquidRendererOptions
        {
            FileProvider = fileProvider,
            ConfigureTemplateContext = configureTemplateContext,
        };
        Email.DefaultRenderer = new LiquidRenderer(Options.Create(options));
    }

    [Fact]
    public void Should_Have_Body_Matching_Template_With_List_Of_Items()
    {
        const string template = "sup {{ Name }} here is a list {% for i in Numbers %}{{ i }}{% endfor %}";

        var email = Email
            .From(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new ViewModel { Name = "LUKE", Numbers = new[] { "1", "2", "3" } });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be("sup LUKE here is a list 123");
    }

    [Fact]
    public void Should_Have_Body_Matching_Template_With_Custom_Context_Values()
    {
        SetupRenderer(new NullFileProvider(), (context, _) =>
        {
            context.SetValue("FirstName", "Samantha");
            context.SetValue("IntegerNumbers", new[] { 3, 2, 1 });
        });

        const string template = "sup {{ FirstName }} here is a list {% for i in IntegerNumbers %}{{ i }}{% endfor %}";

        var email = Email
            .From(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new ViewModel { Name = "LUKE", Numbers = new[] { "1", "2", "3" } });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be("sup Samantha here is a list 321");
    }

    [Fact]
    public void Should_Be_Able_To_Use_Project_Layout()
    {
        SetupRenderer(new PhysicalFileProvider(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName, "EmailTemplates")));

        const string template = @"{% layout '_layout.liquid' %}
sup {{ Name }} here is a list {% for i in Numbers %}{{ i }}{% endfor %}";

        var email = new Email(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new ViewModel { Name = "LUKE", Numbers = new[] { "1", "2", "3" } });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be($"<h1>Hello!</h1>{Environment.NewLine}<div>{Environment.NewLine}sup LUKE here is a list 123</div>");
    }

    [Fact]
    public void Should_Be_Able_To_Use_Embedded_Layout()
    {
        SetupRenderer(new EmbeddedFileProvider(typeof(LiquidTests).Assembly, "FluentEmailer.Liquid.Tests.EmailTemplates"));

        const string template = @"{% layout '_embedded.liquid' %}
sup {{ Name }} here is a list {% for i in Numbers %}{{ i }}{% endfor %}";

        var email = new Email(_fixture.FromEmail)
            .To(_fixture.ToEmail)
            .Subject(_fixture.Subject)
            .UsingTemplate(template, new ViewModel { Name = "LUKE", Numbers = new[] { "1", "2", "3" } });

        email.Data.Body
            .Should().NotBeNullOrEmpty()
            .And.Be($"<h2>Hello!</h2>{Environment.NewLine}<div>{Environment.NewLine}sup LUKE here is a list 123</div>");
    }
}
