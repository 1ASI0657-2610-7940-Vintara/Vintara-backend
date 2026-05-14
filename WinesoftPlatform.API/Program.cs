using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Shared.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Interfaces.ASAP.Configuration;

using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Inventory.Application.Internal.CommandServices;
using WinesoftPlatform.API.Inventory.Application.Internal.QueryServices;
using WinesoftPlatform.API.Inventory.Infrastructure.Persistence.Repositories;
using WinesoftPlatform.API.Analytics.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using WinesoftPlatform.API.Analytics.Infrastructure.Services;
using WinesoftPlatform.API.Authentication.application.@internal.commandservices;
using WinesoftPlatform.API.Authentication.application.@internal.queryservices;
// Importación del nuevo Bounded Context de Purchase
using WinesoftPlatform.API.Purchase.Infrastructure.Interfaces.ASP.Configuration.Extensions; 

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// -----------------------
// CORS: Política única corregida
// -----------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalAndNetlify", policy =>
    {
        policy.WithOrigins(
                "https://winesoft-frontend.vercel.app",
                "https://winesoft-platform.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Importante para cookies
    });
});

// -----------------------
// Controllers
// -----------------------
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WineSoft Platform API",
        Version = "v1",
        Description = "API for the WineSoft inventory and order management platform.",
    });
    options.EnableAnnotations();
});
builder.Services.AddOpenApi();

// -----------------------
// Database
// -----------------------
if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<AppDbContext>(options => {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connectionString is null) 
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
else if (builder.Environment.IsProduction())
    builder.Services.AddDbContext<AppDbContext>(options =>
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

// -----------------------
// Dependency Injection
// -----------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Purchase Context (Aquí estaba el problema, ahora usamos la extensión)
builder.AddPurchaseContextServices();

// Inventory Context
builder.Services.AddScoped<ISupplyRepository, SupplyRepository>();
builder.Services.AddScoped<ISupplyCommandService, SupplyCommandService>();
builder.Services.AddScoped<ISupplyQueryService, SupplyQueryService>();

// IAM / Auth Context
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthCommandService, AuthCommandService>();
builder.Services.AddScoped<IAuthQueryService, AuthQueryService>();

// Analytics Context
builder.AddAnalyticsContextServices();
builder.Services.AddLocalization();
builder.Services.AddScoped<IAnalyticsReportBuilder, QuestPdfAnalyticsReportBuilder>();

var app = builder.Build();

// -----------------------
// Middleware
// -----------------------
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

// Move CORS middleware to run before HTTPS redirection so redirect/actual responses include CORS headers
app.UseCors("AllowLocalAndNetlify");

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Log the exception but don't stop the application from starting.
        // This helps when the DB server (MySQL) is not available during local development.
        Console.WriteLine("Warning: could not ensure DB is created at startup. Exception: \n" + ex);
    }
}

app.UseAuthorization();
app.MapControllers();
app.Run();