using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Referrals;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class ReferralCampaignMap : IEntityTypeConfiguration<ReferralCampaign>
{
    public void Configure(EntityTypeBuilder<ReferralCampaign> builder)
    {
        builder.ToTable("ReferralCampaigns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.DualWriteColumn(x => x.DescriptionHtml, writesTo: "Description", mirrorsTo: "DescriptionHtml");

        builder.DualWriteColumn(x => x.EmailTextHtml, writesTo: "EmailText", mirrorsTo: "EmailTextHtml");

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Name)
            .HasMaxLength(255);
    }
}
