using System;
using System.Collections.Generic;
using System.Text;

namespace ODK.Core.Chapters
{
    public class ChapterTrialSettings
    {
        public ChapterTrialSettings(Guid chapterId, int trialPeriodMonths)
        {
            ChapterId = chapterId;
            TrialPeriodMonths = trialPeriodMonths;
        }

        public Guid ChapterId { get; }

        public int TrialPeriodMonths { get; }
    }
}
