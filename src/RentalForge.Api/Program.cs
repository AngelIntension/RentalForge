using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
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

// Register Npgsql data source with PostgreSQL enum mapping
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<MpaaRating>("mpaa_rating");
var dataSource = dataSourceBuilder.Build();

// Register DvdrentalContext with Npgsql
builder.Services.AddDbContext<DvdrentalContext>(options =>
    options.UseNpgsql(dataSource));

// Dev data seeder (used by --seed CLI argument)
builder.Services.AddScoped<DevDataSeeder>();

// Customer service
builder.Services.AddScoped<ICustomerService, CustomerService>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
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
