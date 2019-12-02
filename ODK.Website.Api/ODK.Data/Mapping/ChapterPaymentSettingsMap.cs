using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterPaymentSettingsMap : SqlMap<ChapterPaymentSettings>
    {
        public ChapterPaymentSettingsMap()
            : base("ChapterAdmin")
        {
            Property(x => x.ChapterId);
            Property(x => x.ApiPublicKey).HasColumnName("PaymentApiPublicKey");
            Property(x => x.ApiSecretKey).HasColumnName("PaymentApiSecretKey");
            Property(x => x.Provider).HasColumnName("PaymentProvider");
        }

        public override ChapterPaymentSettings Read(IDataReader reader)
        {
            return new ChapterPaymentSettings
            (
                chapterId: reader.GetGuid(0),
                apiPublicKey: reader.GetStringOrDefault(1),
                apiSecretKey: reader.GetStringOrDefault(2),
                provider: reader.GetStringOrDefault(3)
            );
        }
    }
}
