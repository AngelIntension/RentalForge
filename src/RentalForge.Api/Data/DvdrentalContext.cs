using Microsoft.EntityFrameworkCore;
using RentalForge.Api.Data.Entities;
using RentalForge.Api.Data.ReferenceData;

namespace RentalForge.Api.Data;

public class DvdrentalContext : DbContext
{
    public DvdrentalContext(DbContextOptions<DvdrentalContext> options)
        : base(options)
    {
    }

    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Film> Films => Set<Film>();
    public DbSet<FilmActor> FilmActors => Set<FilmActor>();
    public DbSet<FilmCategory> FilmCategories => Set<FilmCategory>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PostgreSQL mpaa_rating enum is configured via UseNpgsql(o => o.MapEnum<MpaaRating>())
        // in Program.cs (EF 9.0+ recommended approach). HasPostgresEnum is no longer needed.

        // Actor
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.ToTable("actor");
            entity.HasKey(e => e.ActorId);
            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(45);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(45);
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
        });

        // Address
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("address");
            entity.HasKey(e => e.AddressId);
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.Address1).HasColumnName("address").HasMaxLength(50);
            entity.Property(e => e.Address2).HasColumnName("address2").HasMaxLength(50);
            entity.Property(e => e.District).HasColumnName("district").HasMaxLength(20);
            entity.Property(e => e.CityId).HasColumnName("city_id").HasColumnType("smallint");
            entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasMaxLength(10);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.City)
                .WithMany(c => c.Addresses)
                .HasForeignKey(e => e.CityId);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(25);
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
        });

        // City
        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("city");
            entity.HasKey(e => e.CityId);
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CityName).HasColumnName("city").HasMaxLength(50);
            entity.Property(e => e.CountryId).HasColumnName("country_id").HasColumnType("smallint");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(e => e.CountryId);
        });

        // Country
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("country");
            entity.HasKey(e => e.CountryId);
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CountryName).HasColumnName("country").HasMaxLength(50);
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
        });

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customer");
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasColumnType("smallint");
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(45);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(45);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.AddressId).HasColumnName("address_id").HasColumnType("smallint");
            entity.Property(e => e.Activebool).HasColumnName("activebool").HasDefaultValue(true);
            entity.Property(e => e.CreateDate).HasColumnName("create_date").HasDefaultValueSql("('now'::text)::date");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
            entity.Property(e => e.Active).HasColumnName("active");

            entity.HasOne(e => e.Store)
                .WithMany(s => s.Customers)
                .HasForeignKey(e => e.StoreId);

            entity.HasOne(e => e.Address)
                .WithMany(a => a.Customers)
                .HasForeignKey(e => e.AddressId);
        });

        // Film
        modelBuilder.Entity<Film>(entity =>
        {
            entity.ToTable("film");
            entity.HasKey(e => e.FilmId);
            entity.Property(e => e.FilmId).HasColumnName("film_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(e => e.ReleaseYear).HasColumnName("release_year");
            entity.Property(e => e.LanguageId).HasColumnName("language_id").HasColumnType("smallint");
            entity.Property(e => e.OriginalLanguageId).HasColumnName("original_language_id").HasColumnType("smallint");
            entity.Property(e => e.RentalDuration).HasColumnName("rental_duration").HasDefaultValue((short)3);
            entity.Property(e => e.RentalRate).HasColumnName("rental_rate").HasColumnType("numeric(4,2)").HasDefaultValue(4.99m);
            entity.Property(e => e.Length).HasColumnName("length");
            entity.Property(e => e.ReplacementCost).HasColumnName("replacement_cost").HasColumnType("numeric(5,2)").HasDefaultValue(19.99m);
            entity.Property(e => e.Rating).HasColumnName("rating").HasDefaultValue(MpaaRating.G);
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
            entity.Property(e => e.SpecialFeatures).HasColumnName("special_features").HasColumnType("text[]");
            entity.Property(e => e.Fulltext).HasColumnName("fulltext").HasColumnType("tsvector")
                .HasDefaultValueSql("''::tsvector");

            entity.HasOne(e => e.Language)
                .WithMany(l => l.Films)
                .HasForeignKey(e => e.LanguageId);

            entity.HasOne(e => e.OriginalLanguage)
                .WithMany(l => l.FilmsOriginalLanguage)
                .HasForeignKey(e => e.OriginalLanguageId);
        });

        // FilmActor (composite PK)
        modelBuilder.Entity<FilmActor>(entity =>
        {
            entity.ToTable("film_actor");
            entity.HasKey(e => new { e.ActorId, e.FilmId });
            entity.Property(e => e.ActorId).HasColumnName("actor_id").HasColumnType("smallint");
            entity.Property(e => e.FilmId).HasColumnName("film_id").HasColumnType("smallint");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Actor)
                .WithMany(a => a.FilmActors)
                .HasForeignKey(e => e.ActorId);

            entity.HasOne(e => e.Film)
                .WithMany(f => f.FilmActors)
                .HasForeignKey(e => e.FilmId);
        });

        // FilmCategory (composite PK)
        modelBuilder.Entity<FilmCategory>(entity =>
        {
            entity.ToTable("film_category");
            entity.HasKey(e => new { e.FilmId, e.CategoryId });
            entity.Property(e => e.FilmId).HasColumnName("film_id").HasColumnType("smallint");
            entity.Property(e => e.CategoryId).HasColumnName("category_id").HasColumnType("smallint");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Film)
                .WithMany(f => f.FilmCategories)
                .HasForeignKey(e => e.FilmId);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.FilmCategories)
                .HasForeignKey(e => e.CategoryId);
        });

        // Inventory
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventory");
            entity.HasKey(e => e.InventoryId);
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.FilmId).HasColumnName("film_id").HasColumnType("smallint");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasColumnType("smallint");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Film)
                .WithMany(f => f.Inventories)
                .HasForeignKey(e => e.FilmId);

            entity.HasOne(e => e.Store)
                .WithMany(s => s.Inventories)
                .HasForeignKey(e => e.StoreId);
        });

        // Language
        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("language");
            entity.HasKey(e => e.LanguageId);
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("character(20)");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payment");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").HasColumnType("smallint");
            entity.Property(e => e.StaffId).HasColumnName("staff_id").HasColumnType("smallint");
            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("numeric(5,2)");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.CustomerId);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.Payments)
                .HasForeignKey(e => e.StaffId);

            entity.HasOne(e => e.Rental)
                .WithMany(r => r.Payments)
                .HasForeignKey(e => e.RentalId);
        });

        // Rental
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.ToTable("rental");
            entity.HasKey(e => e.RentalId);
            entity.Property(e => e.RentalId).HasColumnName("rental_id");
            entity.Property(e => e.RentalDate).HasColumnName("rental_date");
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").HasColumnType("smallint");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.StaffId).HasColumnName("staff_id").HasColumnType("smallint");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Inventory)
                .WithMany(i => i.Rentals)
                .HasForeignKey(e => e.InventoryId);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Rentals)
                .HasForeignKey(e => e.CustomerId);

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.Rentals)
                .HasForeignKey(e => e.StaffId);
        });

        // Staff
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("staff");
            entity.HasKey(e => e.StaffId);
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(45);
            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(45);
            entity.Property(e => e.AddressId).HasColumnName("address_id").HasColumnType("smallint");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(50);
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasColumnType("smallint");
            entity.Property(e => e.Active).HasColumnName("active").HasDefaultValue(true);
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(16);
            entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(40);
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");
            entity.Property(e => e.Picture).HasColumnName("picture");

            entity.HasOne(e => e.Address)
                .WithMany(a => a.Staff)
                .HasForeignKey(e => e.AddressId);

            entity.HasOne(e => e.Store)
                .WithMany(s => s.Staff)
                .HasForeignKey(e => e.StoreId);
        });

        // Store
        modelBuilder.Entity<Store>(entity =>
        {
            entity.ToTable("store");
            entity.HasKey(e => e.StoreId);
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ManagerStaffId).HasColumnName("manager_staff_id").HasColumnType("smallint");
            entity.Property(e => e.AddressId).HasColumnName("address_id").HasColumnType("smallint");
            entity.Property(e => e.LastUpdate).HasColumnName("last_update").HasDefaultValueSql("now()");

            entity.HasOne(e => e.ManagerStaff)
                .WithMany(s => s.ManagedStores)
                .HasForeignKey(e => e.ManagerStaffId);

            entity.HasOne(e => e.Address)
                .WithMany(a => a.Stores)
                .HasForeignKey(e => e.AddressId);
        });

        // Reference data seeding (applied by both EnsureCreatedAsync and migrations)
        modelBuilder.Entity<Language>().HasData(LanguageData.GetAll());
        modelBuilder.Entity<Category>().HasData(CategoryData.GetAll());
        modelBuilder.Entity<Country>().HasData(CountryData.GetAll());
        modelBuilder.Entity<City>().HasData(CityData.GetAll());
    }
}
