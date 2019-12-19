using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Mails.MailChimp;

namespace ODK.Services.Mails
{
    public class MailProviderFactory : IMailProviderFactory
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        public MailProviderFactory(IChapterRepository chapterRepository, IMemberRepository memberRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task<IMailProvider> Create(Chapter chapter)
        {
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapter.Id);
            switch (emailSettings.EmailProvider)
            {
                case "MailChimp":
                    return new MailChimpMailProvider(_chapterRepository, _memberRepository);
            }

            throw new NotSupportedException();
        }

        public async Task<IMailProvider> Create(Guid chapterId)
        {
            Chapter chapter = await _chapterRepository.GetChapter(chapterId);
            return await Create(chapter);
        }
    }
}
