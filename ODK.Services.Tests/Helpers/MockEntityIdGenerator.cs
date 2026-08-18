using System;
using ODK.Data.Core;

namespace ODK.Services.Tests.Helpers;

internal class MockEntityIdGenerator : IEntityIdGenerator
{
    private readonly Func<Guid> _factory;

    internal MockEntityIdGenerator(Func<Guid>? factory = null)
    {
        _factory = factory ?? (() => Guid.NewGuid());
    }

    public Guid Next() => _factory();
}
