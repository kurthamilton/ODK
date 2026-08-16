using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ReferralMap : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.ToTable("Referrals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.EmailAddress)
            .HasMaxLength(255);

        builder.Property(x => x.Id)
            .HasColumnName("ReferralId");

        builder.HasRenamedIdColumn();

        // Cascade from the campaign: a deleted campaign takes its referrals with it, they have no meaning
        // without it. Restrict from the member so deleting a member can't silently drop campaign history.
        builder.HasOne<ReferralCampaign>()
            .WithMany()
            .HasForeignKey(x => x.ReferralCampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ReferralCampaignId);
    }
}
