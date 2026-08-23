using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Aim> Aims => Set<Aim>();

    public DbSet<BlacklistedToken> BlacklistedTokens => Set<BlacklistedToken>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Currency> Currencies => Set<Currency>();

    public DbSet<Frequency> Frequencies => Set<Frequency>();

    public DbSet<IntervalUnit> IntervalUnits => Set<IntervalUnit>();

    public DbSet<PlannedTransaction> PlannedTransactions => Set<PlannedTransaction>();

    public DbSet<Source> Sources => Set<Source>();

    public DbSet<SourceAim> SourceAims => Set<SourceAim>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
