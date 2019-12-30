using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Mails.MailChimp;
using ODK.Services.Mails.SendInBlue;

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

        public async Task<IMailProvider> Create(Guid chapterId)
        {
            Chapter chapter = await _chapterRepository.GetChapter(chapterId);
            return await Create(chapter);
        }

        public async Task<IMailProvider> Create(Chapter chapter)
        {
            ChapterEmailProviderSettings emailSettings = await _chapterRepository.GetChapterEmailProviderSettings(chapter.Id);
            return Create(chapter, emailSettings);
        }

        public IMailProvider Create(Chapter chapter, ChapterEmailProviderSettings emailSettings)
        {
            switch (emailSettings.EmailProvider)
            {
                case MailChimpMailProvider.ProviderName:
                    return new MailChimpMailProvider(emailSettings, chapter, _chapterRepository, _memberRepository);
                case SendInBlueMailProvider.ProviderName:
                    return new SendInBlueMailProvider(emailSettings, chapter, _chapterRepository, _memberRepository);
            }

            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<string>> GetProviders()
        {
            return Task.FromResult<IReadOnlyCollection<string>>(new[]
            {
                SendInBlueMailProvider.ProviderName
            });
        }
    }
}
