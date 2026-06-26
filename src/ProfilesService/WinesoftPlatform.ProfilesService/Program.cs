using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WinesoftPlatform.API.Profiles.Application.Internal.CommandServices;
using WinesoftPlatform.API.Profiles.Application.Internal.QueryServices;
using WinesoftPlatform.API.Profiles.Domain.Repositories;
using WinesoftPlatform.API.Profiles.Domain.Services;
using WinesoftPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;
using WinesoftPlatform.API.Shared.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Interfaces.ASAP.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using WinesoftPlatform.ProfilesService.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Diagnostics;
using Serilog;
using WinesoftPlatform.API.Shared.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/profiles-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}");
});


// Controllers
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WineSoft Profiles API",
        Version = "v1",
        Description = "Profiles Service for the WineSoft platform.",
    });
    options.EnableAnnotations();
});
builder.Services.AddOpenApi();

// Token Validation
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured.");
}
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured.");
}
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Database
if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<ProfilesDbContext>(options => {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connectionString is null) 
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
else if (builder.Environment.IsProduction())
    builder.Services.AddDbContext<ProfilesDbContext>(options =>
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionStringTemplate = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionStringTemplate)) 
            throw new Exception("Database connection string template is not set in the configuration.");
        var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("Database connection string is not set in the configuration.");
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error)
            .EnableDetailedErrors();
    });

builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ProfilesDbContext>());

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileCommandService, ProfileCommandService>();
builder.Services.AddScoped<IProfileQueryService, ProfileQueryService>();

builder.Services.AddHealthChecks()
    .AddCheck<DbHealthCheck<ProfilesDbContext>>("database");

var app = builder.Build();

// Correlation ID Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.MapHealthChecks("/health");

const int maxDatabaseInitAttempts = 12;
var databaseInitDelay = TimeSpan.FromSeconds(5);

for (var attempt = 1; attempt <= maxDatabaseInitAttempts; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
        dbContext.Database.Migrate();
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: could not run migrations on DB at startup (attempt {attempt}/{maxDatabaseInitAttempts}). Exception: \n{ex}");

        if (attempt == maxDatabaseInitAttempts)
            throw;

        await Task.Delay(databaseInitDelay);
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
