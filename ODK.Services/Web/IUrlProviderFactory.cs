namespace ODK.Services.Web;

public interface IUrlProviderFactory
{
    IUrlProvider Create(ServiceRequest request);
}
