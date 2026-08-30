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

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.DescriptionHtml)
            .HasColumnName("Description");

        builder.Property(x => x.EmailTextHtml)
            .HasColumnName("EmailText");

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Name)
            .HasMaxLength(255);
    }
}
