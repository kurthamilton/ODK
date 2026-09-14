using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Tests.Routes;

[Parallelizable]
public static class OdkRoutesSignUpDestinationTests
{
    [Test]
    public static void SignUpDestination_CreateGroup_ReturnsTheCreateGroupForm()
    {
        // Arrange
        var routes = new OdkRoutes(PlatformType.GroupSquirrel);

        // Act
        var result = routes.SignUpDestination(SignUpIntentType.CreateGroup);

        // Assert
        result.Should().Be("/my/groups/new");
    }

    [Test]
    public static void SignUpDestination_NoIntent_ReturnsNull()
    {
        /* Arrange - null leaves the caller's own destination alone, which is the login page a sign-up
           already ends at. */
        var routes = new OdkRoutes(PlatformType.GroupSquirrel);

        // Act
        var result = routes.SignUpDestination(intent: null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public static void SignUpDestination_None_ReturnsNull()
    {
        /* Arrange - None is never stored, but it is the value an unparseable query string binds to, so it
           has to read as "nothing stated" rather than falling into a destination. */
        var routes = new OdkRoutes(PlatformType.GroupSquirrel);

        // Act
        var result = routes.SignUpDestination(SignUpIntentType.None);

        // Assert
        result.Should().BeNull();
    }
}
