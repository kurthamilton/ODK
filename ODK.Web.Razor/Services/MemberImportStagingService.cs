using Microsoft.Extensions.Caching.Memory;
using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Services;

public class MemberImportStagingService : IMemberImportStagingService
{
    private const string KeyPrefix = "member-import:";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    private readonly IMemoryCache _cache;

    public MemberImportStagingService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string Stage(IReadOnlyCollection<MemberImportModel> members)
    {
        var token = Guid.NewGuid().ToString("N");
        _cache.Set(Key(token), members, Lifetime);
        return token;
    }

    public IReadOnlyCollection<MemberImportModel>? Retrieve(string? token)
        => !string.IsNullOrEmpty(token)
            && _cache.TryGetValue(Key(token), out IReadOnlyCollection<MemberImportModel>? members)
            ? members
            : null;

    public void Remove(string token) => _cache.Remove(Key(token));

    private static string Key(string token) => KeyPrefix + token;
}
