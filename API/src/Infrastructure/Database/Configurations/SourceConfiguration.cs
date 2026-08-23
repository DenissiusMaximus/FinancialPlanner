using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Sources__3214EC07E31F54A0");

        entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);

        entity.HasOne(d => d.Currency).WithMany(p => p.Sources)
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Sources__Currenc__4D94879B");

        entity.HasOne(d => d.User).WithMany(p => p.Sources)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Sources__UserId__4CA06362");
    }
}
