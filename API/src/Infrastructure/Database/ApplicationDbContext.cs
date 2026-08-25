using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => EnsureUtc(v),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? EnsureUtc(v.Value) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
