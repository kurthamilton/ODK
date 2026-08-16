using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPaymentAccountMap : IEntityTypeConfiguration<ChapterPaymentAccount>
{
    public void Configure(EntityTypeBuilder<ChapterPaymentAccount> builder)
    {
        builder.ToTable("ChapterPaymentAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardPaymentsEnabledUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.IdentityDocumentsProvidedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.OnboardingCompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        /* Both stated rather than left to convention, which would cascade: the database restricts, and a
           payment account is not something to delete as a side effect of removing a group or a member. */
        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterPaymentAccount>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SitePaymentSettings>()
            .WithMany()
            .HasForeignKey(x => x.SitePaymentSettingId);
    }
}