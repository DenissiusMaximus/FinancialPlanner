using System.Text;
using API;
using API.Domain.BalanceManagement;
using API.Domain.Calculator;
using API.Extensions;
using API.Filters;
using API.Services;
using API.Services.Aim;
using API.Services.Category;
using API.Services.Currency;
using API.Services.Frequency;
using API.Services.InteralUnit;
using API.Services.Jwt;
using API.Services.Logging;
using API.Services.PlannedTransaction;
using API.Services.Source;
using API.Services.Transaction;
using API.Services.User;
using API.Utils;
using API.Utils.ExceptionHandler;
using API.Utils.JwtProvider;
using API.Utils.Notification;
using API.Utils.UserContext;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

MapConfig.Configure();

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()?
    .Select(origin => origin.TrimEnd('/'))
    .ToArray() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyHeader() 
              .AllowAnyMethod();
    });
});


builder.Host.UseSerilog(((context, configuration) => configuration
        .WriteTo.Console()
        .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
        .ReadFrom.Configuration(context.Configuration)
    ));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.Decorate<IJwtService, JwtLoggingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Decorate<IUserService, UserLoggingService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISourceService, SourceService>();
builder.Services.AddScoped<IFrequencyService, FrequencyService>();
builder.Services.AddScoped<IIntervalUnitService, IntervalUnitService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ITransactionTypeService, TransactionTypeService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPlannedTransactionService, PlannedTransactionService>();
builder.Services.AddScoped<IAimService, AimService>();
builder.Services.AddScoped<IAimProgressCalculator, AimProgressCalculator>();

builder.Services.AddScoped<IBalanceManagementService, BalanceManagementService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<NotificationContext>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

builder.Services.AddControllers(options => { options.Filters.Add<NotificationFilter>(); });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretAccess"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});


app.UseExceptionHandler();

app.UseRouting(); 

app.UseCors("AllowAll");

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Map("/ui", () => Results.Redirect("/swagger/index.html"));

app.Run();
