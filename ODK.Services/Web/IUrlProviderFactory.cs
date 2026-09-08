using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Web;

public interface IUrlProviderFactory
{
    /// <summary>
    /// URLs about <paramref name="chapter"/>, or about the site this request is served as where it is null.
    /// The group is stated rather than read off the request because a caller's group is not always the
    /// request's - several emails take theirs as a parameter - and because it is the same group the send is
    /// addressed as, which is what keeps a link in the body and the address it came from naming one site.
    /// </summary>
    IUrlProvider Create(IServiceRequest request, Chapter? chapter);

    /// <summary>
    /// The same, for a caller that has already resolved the platform - <c>EmailService</c>, which resolves it
    /// once for the whole render.
    /// </summary>
    IUrlProvider Create(PlatformType platform);
}
