using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0750273533");

        entity.HasIndex(e => e.Email, "UQ__Users__A9D105340A5F25EB").IsUnique();

        entity.Property(e => e.Email)
            .HasMaxLength(255)
            .IsUnicode(true);
        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);
        entity.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .IsUnicode(true);
    }
}
