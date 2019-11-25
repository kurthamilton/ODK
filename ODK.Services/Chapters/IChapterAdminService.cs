using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Chapters
{
    public interface IChapterAdminService
    {
        Task<IReadOnlyCollection<Chapter>> GetChapters(Guid memberId);
    }
}
