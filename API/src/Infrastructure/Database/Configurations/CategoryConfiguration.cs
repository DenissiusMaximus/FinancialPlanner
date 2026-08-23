using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC070D88E6BD");

        entity.Property(e => e.Name)
            .HasMaxLength(255)
            .IsUnicode(true);

        entity.HasOne(d => d.User).WithMany(p => p.Categories)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Categorie__UserI__440B1D61");
    }
}
