using ODK.Data.Core;
using ODK.Data.EntityFramework;

namespace ODK.Services.Tests.Helpers;

internal static class MockUnitOfWorkFactory
{
    internal static IUnitOfWork Create(
        MockOdkContext? context = null,
        IEntityIdGenerator? idEntityIdGenerator = null)
    {
        context ??= new MockOdkContext();
        context.SaveChanges();
        return new UnitOfWork(context, idEntityIdGenerator ?? new MockEntityIdGenerator());
    }
}