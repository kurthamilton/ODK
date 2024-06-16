using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ODK.Core.Events;
using ODK.Services.Authorization;

namespace ODK.Services.Tests.Events;

public static class EventServiceTests
{
    private static IAuthorizationService CreateMockAuthorizationService()
    {
        Mock<IAuthorizationService> mock = new Mock<IAuthorizationService>();

        return mock.Object;
    }

    private static IEventRepository CreateMockEventRepository(IEnumerable<EventResponse>? responses = null)
    {
        Mock<IEventRepository> mock = new Mock<IEventRepository>();

        mock.Setup(x => x.GetEventResponsesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(responses?.ToArray() ?? new EventResponse[0]);

        mock.Setup(x => x.GetMemberResponsesAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
            .ReturnsAsync(responses?.ToArray() ?? new EventResponse[0]);

        return mock.Object;
    }
}
