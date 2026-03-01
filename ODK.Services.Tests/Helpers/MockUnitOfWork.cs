using ODK.Data.Core;
using ODK.Data.EntityFramework;

namespace ODK.Services.Tests.Helpers;

internal static class MockUnitOfWork
{
    internal static IUnitOfWork Create(MockOdkContext? context = null)
    {
        context ??= new MockOdkContext();
        context.SaveChanges();
        return new UnitOfWork(context);
    }
}