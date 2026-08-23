using FinancialPlanner.Application.Abstractions;
using FinancialPlanner.Application.Common.Mapping;
using FinancialPlanner.Domain.Repositories;
using FinancialPlanner.Domain.Services;
using FinancialPlanner.Infrastructure.Database;
using FinancialPlanner.Infrastructure.Database.Repositories;
using FinancialPlanner.Infrastructure.Security;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MapsterMapper;

namespace FinancialPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbConnection(configuration);
        services.AddValidators(configuration);
        services.ConfigureMapster();
        services.AddRepositories();
        services.AddDomainServices();
        services.AddHandlers();
        services.AddSecurity(configuration);

        return services;
    }

    public static IServiceCollection AddDbConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. Configure it via User Secrets or environment variable ConnectionStrings__DefaultConnection.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(IPasswordHasher).Assembly);

        return services;
    }

    public static IServiceCollection ConfigureMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(IPasswordHasher).Assembly);

        PatchMapperConfig.Configure();

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        services.AddScoped<IPatchMapper, PatchMapper>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAimRepository, AimRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFrequencyRepository, FrequencyRepository>();
        services.AddScoped<IIntervalUnitRepository, IntervalUnitRepository>();
        services.AddScoped<IPlannedTransactionRepository, PlannedTransactionRepository>();
        services.AddScoped<ISourceRepository, SourceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBlacklistedTokenRepository, BlacklistedTokenRepository>();

        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IBalanceManager, BalanceManager>();
        services.AddSingleton<IAimProgressCalculator, AimProgressCalculator>();

        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IPasswordHasher>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Handler")))
            .AsSelf()
            .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        return services;
    }
}
