namespace ODK.Services.Web;

public interface IUrlProviderFactory
{
    Task<IUrlProvider> Create(ServiceRequest request);
}
