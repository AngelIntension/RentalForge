using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
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

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<DvdrentalContext>()
    .AddDefaultTokenProviders();

// JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrWhiteSpace(jwtKey))
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
}

builder.Services.AddAuthorization();

// Dev data seeder (used by --seed CLI argument)
builder.Services.AddScoped<DevDataSeeder>();

// Customer service
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Film service
builder.Services.AddScoped<IFilmService, FilmService>();

// Rental service
builder.Services.AddScoped<IRentalService, RentalService>();

// Auth service
builder.Services.AddScoped<IAuthService, AuthService>();

// FluentValidation (validators injected into service layer, no auto-validation)
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

// Controller-based routing (constitution v1.3.0)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSwaggerGen(options =>
    options.EnableAnnotations());

// Rate limiting (permit limits configurable for test override)
var loginLimit = builder.Configuration.GetValue("RateLimiting:LoginPermitLimit", 5);
var registerLimit = builder.Configuration.GetValue("RateLimiting:RegisterPermitLimit", 3);
var refreshLimit = builder.Configuration.GetValue("RateLimiting:RefreshPermitLimit", 10);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(
                System.Threading.RateLimiting.MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        return ValueTask.CompletedTask;
    };
    options.AddFixedWindowLimiter("auth-login", limiter =>
    {
        limiter.PermitLimit = loginLimit;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("auth-register", limiter =>
    {
        limiter.PermitLimit = registerLimit;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("auth-refresh", limiter =>
    {
        limiter.PermitLimit = refreshLimit;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

// CORS for frontend dev server
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()));

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

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible to WebApplicationFactory
public partial class Program;
