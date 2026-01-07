namespace ODK.Data.Core;

public interface IUnitOfWorkFactory : IDisposable
{
    IUnitOfWork Create();
}
