using ODK.Core.Platforms;
using ODK.Data.Core;

namespace ODK.Data.EntityFramework;
public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly List<OdkContext> _created = new();
    private readonly IPlatformProvider _platformProvider;
    private readonly OdkContextSettings _settings;

    public UnitOfWorkFactory(OdkContextSettings settings, IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
        _settings = settings;
    }

    public IUnitOfWork Create()
    {
        var context = new OdkContext(_settings);
        _created.Add(context);
        return new UnitOfWork(context, _platformProvider);
    }

    public void Dispose()
    {
        foreach (var context in _created)
        {
            context.Dispose();
        }
    }
}
