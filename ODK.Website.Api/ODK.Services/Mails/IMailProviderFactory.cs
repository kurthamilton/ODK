using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Mails
{
    public interface IMailProviderFactory
    {
        Task<IMailProvider> Create(Chapter chapter);

        Task<IMailProvider> Create(Guid chapterId);
    }
}
