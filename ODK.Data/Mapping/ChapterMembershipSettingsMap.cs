using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterMembershipSettingsMap : SqlMap<ChapterMembershipSettings>
    {
        public ChapterMembershipSettingsMap()
            : base("ChapterSettings")
        {
            Property(x => x.ChapterId);
            Property(x => x.TrialPeriodMonths);
            Property(x => x.MembershipDisabledAfterDaysExpired);
        }

        public override ChapterMembershipSettings Read(IDataReader reader)
        {
            return new ChapterMembershipSettings
            (
                chapterId: reader.GetGuid(0),
                trialPeriodMonths: reader.GetInt32(1),
                membershipDisabledAfterDaysExpired: reader.GetInt32(2)
            );
        }
    }
}
