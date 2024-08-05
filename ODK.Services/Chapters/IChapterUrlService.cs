using ODK.Core.Chapters;

namespace ODK.Services.Chapters;

public interface IChapterUrlService
{
    string GetChapterUrl(Chapter? chapter, string path, IDictionary<string, string> parameters);    
}
