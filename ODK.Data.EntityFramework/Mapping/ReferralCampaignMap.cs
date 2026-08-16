using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Referrals;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ReferralCampaignMap : IEntityTypeConfiguration<ReferralCampaign>
{
    public void Configure(EntityTypeBuilder<ReferralCampaign> builder)
    {
        builder.ToTable("ReferralCampaigns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("ReferralCampaignId");

        builder.HasRenamedIdColumn();

        builder.Property(x => x.Name)
            .HasMaxLength(255);
    }
}
