using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class FrequencyConfiguration : IEntityTypeConfiguration<Frequency>
{
    public void Configure(EntityTypeBuilder<Frequency> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Frequenc__3214EC0734943CC1");

        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);

        entity.HasOne(d => d.IntervalUnitNavigation).WithMany(p => p.Frequencies)
            .HasForeignKey(d => d.IntervalUnitId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Frequenci__Inter__03F0984C");

        entity.HasOne(d => d.User).WithMany(p => p.Frequencies)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK__Frequenci__UserI__01142BA1");

        entity.HasData(
            new Frequency { Id = 1, Name = "Week", IntervalValue = 1, UserId = null, IntervalUnitId = 2 },
            new Frequency { Id = 2, Name = "Two Weeks", IntervalValue = 2, UserId = null, IntervalUnitId = 2 },
            new Frequency { Id = 3, Name = "Day", IntervalValue = 1, UserId = null, IntervalUnitId = 1 },
            new Frequency { Id = 4, Name = "Month", IntervalValue = 1, UserId = null, IntervalUnitId = 3 },
            new Frequency { Id = 5, Name = "Year", IntervalValue = 1, UserId = null, IntervalUnitId = 4 }
        );
    }
}
