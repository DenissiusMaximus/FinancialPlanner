using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class AimConfiguration : IEntityTypeConfiguration<Aim>
{
    public void Configure(EntityTypeBuilder<Aim> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Aims__3214EC0793FD8D38");

        entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);

        entity.HasOne(d => d.Currency).WithMany(p => p.Aims)
            .HasForeignKey(d => d.CurrencyId)
            .HasConstraintName("FK__Aims__CurrencyId__656C112C");

        entity.HasOne(d => d.User).WithMany(p => p.Aims)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Aims__UserId__5070F446");
    }
}
