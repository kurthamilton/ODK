using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterTrialSettingsMap : SqlMap<ChapterTrialSettings>
    {
        public ChapterTrialSettingsMap()
            : base("ChapterSettings")
        {
            Property(x => x.ChapterId);
            Property(x => x.TrialPeriodMonths);
        }

        public override ChapterTrialSettings Read(IDataReader reader)
        {
            return new ChapterTrialSettings
            (
                chapterId: reader.GetGuid(0),
                trialPeriodMonths: reader.GetInt32(1)
            );
        }
    }
}
