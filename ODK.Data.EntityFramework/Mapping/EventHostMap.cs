using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.EntityFramework.Converters;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class EventHostMap : IEntityTypeConfiguration<EventHost>
{
    public void Configure(EntityTypeBuilder<EventHost> builder)
    {
        builder.ToTable("EventHosts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("EventHostId");        

        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}

