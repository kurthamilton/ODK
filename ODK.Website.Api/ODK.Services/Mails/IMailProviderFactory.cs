using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Mails
{
    public interface IMailProviderFactory
    {
        Task<IMailProvider> Create(Guid chapterId);

        Task<IMailProvider> Create(Chapter chapter);

        IMailProvider Create(Chapter chapter, ChapterEmailSettings emailSettings);

        Task<IReadOnlyCollection<string>> GetProviders();
    }
}
