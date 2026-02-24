using ODK.Data.Core;

namespace ODK.Data.EntityFramework;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly List<OdkContext> _created = new();
    private readonly OdkContextSettings _settings;

    public UnitOfWorkFactory(OdkContextSettings settings)
    {
        _settings = settings;
    }

    public IUnitOfWork Create()
    {
        var context = new OdkContext(_settings);
        _created.Add(context);
        return new UnitOfWork(context);
    }

    public void Dispose()
    {
        foreach (var context in _created)
        {
            context.Dispose();
        }
    }
}