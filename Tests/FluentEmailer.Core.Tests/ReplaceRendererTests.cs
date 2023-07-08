using FluentEmailer.Core.Defaults;
using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;

namespace FluentEmailer.Core.Tests;

public class ReplaceRendererTests
{
    [Fact]
    public void Should_Pass_When_Model_Property_Value_Is_Null()
    {
        ITemplateRenderer templateRenderer = new ReplaceRenderer();

        var address = new Address("james@test.com", "james");
        const string template = "this is name: ##Name##";

        templateRenderer.Parse(template, address)
            .Should().Be("this is name: james");

        address = new Address("james@test.com");
        templateRenderer.Parse(template, address)
            .Should().Be("this is name: ");
    }
}