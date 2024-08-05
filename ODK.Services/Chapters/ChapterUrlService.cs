using ODK.Core.Chapters;
using ODK.Core.Utils;
using ODK.Services.Platforms;

namespace ODK.Services.Chapters;

public class ChapterUrlService : IChapterUrlService
{
    private readonly IPlatformProvider _platformProvider;

    public ChapterUrlService(IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    public string GetChapterUrl(Chapter? chapter, string path, IDictionary<string, string> parameters)
    {
        var baseUrl = _platformProvider.GetBaseUrl();

        if (chapter == null)
        {
            path = path.Replace("/{chapter.name}", "");
        }
        else
        {
            if (!parameters.ContainsKey("chapter.name"))
            {
                parameters.Add("chapter.name", chapter.Name);
            }
        }

        return baseUrl + path.Interpolate(parameters);
    }
}
