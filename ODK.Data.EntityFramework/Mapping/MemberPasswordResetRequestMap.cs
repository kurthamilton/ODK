using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberPasswordResetRequestMap : IEntityTypeConfiguration<MemberPasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<MemberPasswordResetRequest> builder)
    {
        builder.ToTable("MemberPasswordResetRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasColumnName("Created");

        builder.Property(x => x.ExpiresUtc)
            .HasColumnName("Expires");

        builder.Property(x => x.Id)
            .HasColumnName("MemberPasswordResetRequestId");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
