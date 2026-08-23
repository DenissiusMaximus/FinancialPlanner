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

        entity.HasData(
            new Currency { Id = 1, Name = "UAH", UsdExchangeRate = 0.0224m },
            new Currency { Id = 2, Name = "USD", UsdExchangeRate = 1.0000m },
            new Currency { Id = 3, Name = "EUR", UsdExchangeRate = 1.1685m }
        );
    }
}
