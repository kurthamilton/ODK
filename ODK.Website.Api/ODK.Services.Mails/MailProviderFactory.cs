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
        private const string MailChimpProviderName = "MailChimp";
        private const string SendInBlueProviderName = "SendInBlue";

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
            ChapterEmailSettings emailSettings = await _chapterRepository.GetChapterEmailSettings(chapter.Id);
            return Create(chapter, emailSettings);
        }

        public IMailProvider Create(Chapter chapter, ChapterEmailSettings emailSettings)
        {
            switch (emailSettings.EmailProvider)
            {
                case MailChimpProviderName:
                    return new MailChimpMailProvider(_chapterRepository, _memberRepository);
                case SendInBlueProviderName:
                    return new SendInBlueMailProvider(_chapterRepository);
            }

            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<string>> GetProviders()
        {
            return Task.FromResult<IReadOnlyCollection<string>>(new[]
            {
                MailChimpProviderName,
                SendInBlueProviderName
            });
        }
    }
}
