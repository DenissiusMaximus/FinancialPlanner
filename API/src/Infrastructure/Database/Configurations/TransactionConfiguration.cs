using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0728D92CEC");

        entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
        entity.Property(e => e.Comment).IsUnicode(true);

        entity.HasOne(d => d.Category).WithMany(p => p.Transactions)
            .HasForeignKey(d => d.CategoryId)
            .HasConstraintName("FK__Transacti__Categ__59063A47");

        entity.HasOne(d => d.Currency).WithMany(p => p.Transactions)
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Transacti__Curre__5BE2A6F2");

        entity.HasOne(d => d.DestinationSource).WithMany(p => p.TransactionDestinationSources)
            .HasForeignKey(d => d.DestinationSourceId)
            .HasConstraintName("FK__Transacti__Desti__5AEE82B9");

        entity.HasOne(d => d.Source).WithMany(p => p.TransactionSources)
            .HasForeignKey(d => d.SourceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Transacti__Sourc__59FA5E80");

        entity.HasOne(d => d.TransactionType).WithMany(p => p.Transactions)
            .HasForeignKey(d => d.TransactionTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Transacti__Trans__5CD6CB2B");

        entity.HasOne(d => d.User).WithMany(p => p.Transactions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__Transacti__UserI__5812160E");
    }
}
