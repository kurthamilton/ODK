using ODK.Core.Chapters;

namespace ODK.Services;

public interface IChapterServiceRequest : IServiceRequest
{
    Chapter Chapter { get; }
}