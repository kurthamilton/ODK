using ODK.Data.Core;

namespace ODK.Data.EntityFramework;

public class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly List<OdkContext> _created = new();
    private readonly IEntityIdGenerator _idGenerator;
    private readonly OdkContextSettings _settings;

    public UnitOfWorkFactory(
        OdkContextSettings settings,
        IEntityIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
        _settings = settings;
    }

    public IUnitOfWork Create()
    {
        var context = new OdkContext(_settings);
        _created.Add(context);
        return new UnitOfWork(context, _idGenerator);
    }

    public void Dispose()
    {
        foreach (var context in _created)
        {
            context.Dispose();
        }
    }
}