using System;

namespace ODK.Core.Chapters
{
    public class ChapterTexts
    {
        public ChapterTexts(Guid chapterId, string registerText, string welcomeText)
        {
            ChapterId = chapterId;
            RegisterText = registerText;
            WelcomeText = welcomeText;
        }

        public Guid ChapterId { get; }

        public string RegisterText { get; }

        public string WelcomeText { get; }
    }
}
