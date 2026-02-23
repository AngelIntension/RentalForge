using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RentalForge.Api.Data;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Data.Seeding;
using RentalForge.Api.Services;
using RentalForge.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Validate connection string at startup (FR-008)
var connectionString = builder.Configuration.GetConnectionString("Dvdrental");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Dvdrental' is missing or empty. " +
        "Set it via user-secrets: dotnet user-secrets set \"ConnectionStrings:Dvdrental\" " +
        "\"Host=localhost;Port=5432;Database=dvdrental;Username=postgres;Password=<your-password>\"");
}

// Register DvdrentalContext with Npgsql — MapEnum on UseNpgsql configures enum mapping
// at both the EF Core and Npgsql layers (EF 9.0+ recommended approach).
// Suppress PendingModelChangesWarning — the dvdrental database was created from an external
// dump, so EF Core's FK naming conventions will always drift from the dump's conventions.
builder.Services.AddDbContext<DvdrentalContext>(options =>
    options.UseNpgsql(connectionString, o => o.MapEnum<MpaaRating>("mpaa_rating"))
        .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning)));

// Dev data seeder (used by --seed CLI argument)
builder.Services.AddScoped<DevDataSeeder>();

// Customer service
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Film service
builder.Services.AddScoped<IFilmService, FilmService>();

// FluentValidation (validators injected into service layer, no auto-validation)
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

// Controller-based routing (constitution v1.3.0)
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
    options.EnableAnnotations());

var app = builder.Build();

// Handle --seed CLI argument (exit without starting web server)
if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
    var force = args.Contains("--force");

    if (force)
        await seeder.SeedForceAsync();
    else
        await seeder.SeedAsync();

    return;
}

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

// Make Program class accessible to WebApplicationFactory
public partial class Program;
