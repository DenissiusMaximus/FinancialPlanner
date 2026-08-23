using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Currenci__3214EC0744C77311");

        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);
        entity.Property(e => e.UsdExchangeRate).HasColumnType("decimal(18, 4)");
    }
}
