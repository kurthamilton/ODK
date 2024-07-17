using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberPropertyMap : IEntityTypeConfiguration<MemberProperty>
{
    public void Configure(EntityTypeBuilder<MemberProperty> builder)
    {
        builder.ToTable("MemberProperties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("MemberPropertyId");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
