using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class BlacklistedTokenConfiguration : IEntityTypeConfiguration<BlacklistedToken>
{
    public void Configure(EntityTypeBuilder<BlacklistedToken> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Blacklis__3214EC078E1AA827");

        entity.Property(e => e.Jti)
            .HasMaxLength(255)
            .IsUnicode(true);
    }
}
