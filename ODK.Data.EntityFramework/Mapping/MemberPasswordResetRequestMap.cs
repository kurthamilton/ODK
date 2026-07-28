using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberPasswordResetRequestMap : IEntityTypeConfiguration<MemberPasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<MemberPasswordResetRequest> builder)
    {
        builder.ToTable("MemberPasswordResetRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<UtcDateTimeConverter>();

        // Transition shadows keep the legacy Created/Expires columns populated until they are dropped.
        builder.Property<DateTime?>("CreatedUtcColumn")
            .HasColumnName("Created")
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property<DateTime?>("ExpiresUtcColumn")
            .HasColumnName("Expires")
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("MemberPasswordResetRequestId");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
