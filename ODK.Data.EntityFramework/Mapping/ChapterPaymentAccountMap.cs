using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPaymentAccountMap : IEntityTypeConfiguration<ChapterPaymentAccount>
{
    public void Configure(EntityTypeBuilder<ChapterPaymentAccount> builder)
    {
        builder.ToTable("ChapterPaymentAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("ChapterPaymentAccountId");

        builder.Property(x => x.OnboardingCompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterPaymentAccount>(x => x.ChapterId);
    }
}
