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
            .HasColumnName("Created")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExpiresUtc)
            .HasColumnName("Expires")
            .HasConversion<UtcDateTimeConverter>();

        // Transition shadows for the UTC column-name standardisation (Created -> CreatedUtc,
        // Expires -> ExpiresUtc).
        builder.Property<DateTime?>("CreatedUtcColumn")
            .HasColumnName("CreatedUtc")
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property<DateTime?>("ExpiresUtcColumn")
            .HasColumnName("ExpiresUtc")
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("MemberPasswordResetRequestId");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
