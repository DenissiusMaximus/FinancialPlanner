using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07B0E1127F");

        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);
    }
}
