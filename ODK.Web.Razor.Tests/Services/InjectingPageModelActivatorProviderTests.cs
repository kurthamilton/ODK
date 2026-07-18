using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ODK.Web.Razor.Attributes;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Tests.Services;

[Parallelizable]
public static class InjectingPageModelActivatorProviderTests
{
    [Test]
    public static void InjectProperties_SetsMarkedProperties_LeavesUnmarkedNull()
    {
        var service = new Service();
        var provider = new ServiceCollection()
            .AddSingleton(service)
            .BuildServiceProvider();
        var target = new Target();

        InjectingPageModelActivatorProvider<OdkInjectAttribute>.InjectProperties(target, provider);

        target.Injected.Should().BeSameAs(service);
        target.NotInjected.Should().BeNull();
    }

    private sealed class Service
    {
    }

    private sealed class Target
    {
        [OdkInject]
        public Service? Injected { get; set; }

        public Service? NotInjected { get; set; }
    }
}
