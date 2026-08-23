using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class SourceAimConfiguration : IEntityTypeConfiguration<SourceAim>
{
    public void Configure(EntityTypeBuilder<SourceAim> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__SourceAi__3214EC07C10B6095");

        entity.HasOne(d => d.Aim).WithMany(p => p.SourceAims)
            .HasForeignKey(d => d.AimId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__SourceAim__AimId__5535A963");

        entity.HasOne(d => d.Source).WithMany(p => p.SourceAims)
            .HasForeignKey(d => d.SourceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__SourceAim__Sourc__5441852A");
    }
}
