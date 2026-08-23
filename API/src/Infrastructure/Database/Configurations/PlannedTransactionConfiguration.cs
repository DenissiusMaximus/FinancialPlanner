using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlanner.Infrastructure.Database.Configurations;

public class PlannedTransactionConfiguration : IEntityTypeConfiguration<PlannedTransaction>
{
    public void Configure(EntityTypeBuilder<PlannedTransaction> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK__PlannedT__3214EC07598C8B65");

        entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
        entity.Property(e => e.Name)
            .HasMaxLength(555)
            .IsUnicode(true);

        entity.HasOne(d => d.Category).WithMany(p => p.PlannedTransactions)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK__PlannedTr__Categ__628FA481");

        entity.HasOne(d => d.Currency).WithMany(p => p.PlannedTransactions)
            .HasForeignKey(d => d.CurrencyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__PlannedTr__Curre__5FB337D6");

        entity.HasOne(d => d.Frequency).WithMany(p => p.PlannedTransactions)
            .HasForeignKey(d => d.FrequencyId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__PlannedTr__Frequ__6477ECF3");

        entity.HasOne(d => d.Source).WithMany(p => p.PlannedTransactions)
            .HasForeignKey(d => d.SourceId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__PlannedTr__Sourc__6383C8BA");

        entity.HasOne(d => d.TransactionType).WithMany(p => p.PlannedTransactions)
            .HasForeignKey(d => d.TransactionTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__PlannedTr__Trans__619B8048");

        entity.HasOne(d => d.User).WithMany(p => p.PlannedTransactions)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK__PlannedTr__UserI__60A75C0F");
    }
}
