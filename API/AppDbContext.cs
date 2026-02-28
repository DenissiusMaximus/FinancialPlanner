using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aim> Aims { get; set; }

    public virtual DbSet<BlacklistedToken> BlacklistedTokens { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Frequency> Frequencies { get; set; }

    public virtual DbSet<PlannedTransaction> PlannedTransactions { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    public virtual DbSet<SourceAim> SourceAims { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aims__3214EC0793FD8D38");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Currency).WithMany(p => p.Aims)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("FK__Aims__CurrencyId__656C112C");

            entity.HasOne(d => d.User).WithMany(p => p.Aims)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Aims__UserId__5070F446");
        });

        modelBuilder.Entity<BlacklistedToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Blacklis__3214EC078E1AA827");

            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.Jti)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC070D88E6BD");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Categories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Categorie__UserI__440B1D61");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Currenci__3214EC0744C77311");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UsdExchangeRate).HasColumnType("decimal(18, 4)");
        });

        modelBuilder.Entity<Frequency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Frequenc__3214EC0734943CC1");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlannedTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlannedT__3214EC07598C8B65");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Name)
                .HasMaxLength(555)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.PlannedTransactions)
                .HasForeignKey(d => d.CategoryId)
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
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sources__3214EC07E31F54A0");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Currency).WithMany(p => p.Sources)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sources__Currenc__4D94879B");

            entity.HasOne(d => d.User).WithMany(p => p.Sources)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sources__UserId__4CA06362");
        });

        modelBuilder.Entity<SourceAim>(entity =>
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
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0728D92CEC");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Comment).IsUnicode(false);

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
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07B0E1127F");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0750273533");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105340A5F25EB").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
