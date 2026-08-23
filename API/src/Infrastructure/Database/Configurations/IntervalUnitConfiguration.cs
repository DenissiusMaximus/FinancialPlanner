using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class IntervalUnitConfiguration : IEntityTypeConfiguration<IntervalUnit>
{
    public void Configure(EntityTypeBuilder<IntervalUnit> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Interval__3213E83F513E4BBA");

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true)
            .HasColumnName("name");

        entity.HasData(
            new IntervalUnit { Id = 1, Name = "Day" },
            new IntervalUnit { Id = 2, Name = "Week" },
            new IntervalUnit { Id = 3, Name = "Month" },
            new IntervalUnit { Id = 4, Name = "Year" }
        );
    }
}
