using FluentEmailer.Core.Models;

namespace FluentEmailer.Core.Tests;

internal static class FluentAssertionsExtensions
{
    internal static void ShouldBeSuccessful(this SendResponse response)
    {
        response
            .Should().NotBeNull()
            .And.BeEquivalentTo(new { Successful = true });
    }

    internal static void ShouldNotBeSuccessful(this SendResponse response)
    {
        response
            .Should().NotBeNull()
            .And.BeEquivalentTo(new { Successful = false });
    }
}
