using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;
using RentalForge.Api.Data.Entities;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentalForge.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:mpaa_rating", "G,NC-17,PG,PG-13,R");

            migrationBuilder.CreateTable(
                name: "actor",
                columns: table => new
                {
                    actor_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    last_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actor", x => x.actor_id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    country_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country", x => x.country_id);
                });

            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    language_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character(20)", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_language", x => x.language_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "city",
                columns: table => new
                {
                    city_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    country_id = table.Column<short>(type: "smallint", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_city", x => x.city_id);
                    table.ForeignKey(
                        name: "FK_city_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "country_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "film",
                columns: table => new
                {
                    film_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    release_year = table.Column<int>(type: "integer", nullable: true),
                    language_id = table.Column<short>(type: "smallint", nullable: false),
                    original_language_id = table.Column<short>(type: "smallint", nullable: true),
                    rental_duration = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)3),
                    rental_rate = table.Column<decimal>(type: "numeric(4,2)", nullable: false, defaultValue: 4.99m),
                    length = table.Column<short>(type: "smallint", nullable: true),
                    replacement_cost = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 19.99m),
                    rating = table.Column<MpaaRating>(type: "mpaa_rating", nullable: true, defaultValue: MpaaRating.G),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    special_features = table.Column<string[]>(type: "text[]", nullable: true),
                    fulltext = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false, defaultValueSql: "''::tsvector")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_film", x => x.film_id);
                    table.ForeignKey(
                        name: "FK_film_language_language_id",
                        column: x => x.language_id,
                        principalTable: "language",
                        principalColumn: "language_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_film_language_original_language_id",
                        column: x => x.original_language_id,
                        principalTable: "language",
                        principalColumn: "language_id");
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "address",
                columns: table => new
                {
                    address_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    district = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    city_id = table.Column<short>(type: "smallint", nullable: false),
                    postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address", x => x.address_id);
                    table.ForeignKey(
                        name: "FK_address_city_city_id",
                        column: x => x.city_id,
                        principalTable: "city",
                        principalColumn: "city_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "film_actor",
                columns: table => new
                {
                    actor_id = table.Column<short>(type: "smallint", nullable: false),
                    film_id = table.Column<short>(type: "smallint", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_film_actor", x => new { x.actor_id, x.film_id });
                    table.ForeignKey(
                        name: "FK_film_actor_actor_actor_id",
                        column: x => x.actor_id,
                        principalTable: "actor",
                        principalColumn: "actor_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_film_actor_film_film_id",
                        column: x => x.film_id,
                        principalTable: "film",
                        principalColumn: "film_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "film_category",
                columns: table => new
                {
                    film_id = table.Column<short>(type: "smallint", nullable: false),
                    category_id = table.Column<short>(type: "smallint", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_film_category", x => new { x.film_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_film_category_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_film_category_film_film_id",
                        column: x => x.film_id,
                        principalTable: "film",
                        principalColumn: "film_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    store_id = table.Column<short>(type: "smallint", nullable: false),
                    first_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    last_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address_id = table.Column<short>(type: "smallint", nullable: false),
                    activebool = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    create_date = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "('now'::text)::date"),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    active = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.customer_id);
                    table.ForeignKey(
                        name: "FK_customer_address_address_id",
                        column: x => x.address_id,
                        principalTable: "address",
                        principalColumn: "address_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    token = table.Column<string>(type: "text", nullable: false),
                    family = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_user_logins_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    film_id = table.Column<short>(type: "smallint", nullable: false),
                    store_id = table.Column<short>(type: "smallint", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK_inventory_film_film_id",
                        column: x => x.film_id,
                        principalTable: "film",
                        principalColumn: "film_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<short>(type: "smallint", nullable: false),
                    staff_id = table.Column<short>(type: "smallint", nullable: false),
                    rental_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payment_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rental",
                columns: table => new
                {
                    rental_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rental_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inventory_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<short>(type: "smallint", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    staff_id = table.Column<short>(type: "smallint", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rental", x => x.rental_id);
                    table.ForeignKey(
                        name: "FK_rental_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rental_inventory_inventory_id",
                        column: x => x.inventory_id,
                        principalTable: "inventory",
                        principalColumn: "inventory_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    staff_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    last_name = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    address_id = table.Column<short>(type: "smallint", nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    store_id = table.Column<short>(type: "smallint", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    username = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    password = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    picture = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.staff_id);
                    table.ForeignKey(
                        name: "FK_staff_address_address_id",
                        column: x => x.address_id,
                        principalTable: "address",
                        principalColumn: "address_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "store",
                columns: table => new
                {
                    store_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    manager_staff_id = table.Column<short>(type: "smallint", nullable: false),
                    address_id = table.Column<short>(type: "smallint", nullable: false),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store", x => x.store_id);
                    table.ForeignKey(
                        name: "FK_store_address_address_id",
                        column: x => x.address_id,
                        principalTable: "address",
                        principalColumn: "address_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_store_staff_manager_staff_id",
                        column: x => x.manager_staff_id,
                        principalTable: "staff",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "category",
                columns: new[] { "category_id", "last_update", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Action" },
                    { 2, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Animation" },
                    { 3, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Children" },
                    { 4, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Classics" },
                    { 5, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Comedy" },
                    { 6, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Documentary" },
                    { 7, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Drama" },
                    { 8, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Family" },
                    { 9, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Foreign" },
                    { 10, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Games" },
                    { 11, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Horror" },
                    { 12, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Music" },
                    { 13, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "New" },
                    { 14, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Sci-Fi" },
                    { 15, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Sports" },
                    { 16, new DateTime(2006, 2, 15, 9, 46, 27, 0, DateTimeKind.Utc), "Travel" }
                });

            migrationBuilder.InsertData(
                table: "country",
                columns: new[] { "country_id", "country", "last_update" },
                values: new object[,]
                {
                    { 1, "Afghanistan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 2, "Algeria", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 3, "American Samoa", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 4, "Angola", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 5, "Anguilla", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 6, "Argentina", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 7, "Armenia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 8, "Australia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 9, "Austria", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 10, "Azerbaijan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 11, "Bahrain", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 12, "Bangladesh", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 13, "Belarus", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 14, "Bolivia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 15, "Brazil", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 16, "Brunei", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 17, "Bulgaria", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 18, "Cambodia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 19, "Cameroon", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 20, "Canada", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 21, "Chad", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 22, "Chile", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 23, "China", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 24, "Colombia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 25, "Congo, The Democratic Republic of the", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 26, "Czech Republic", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 27, "Dominican Republic", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 28, "Ecuador", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 29, "Egypt", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 30, "Estonia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 31, "Ethiopia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 32, "Faroe Islands", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 33, "Finland", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 34, "France", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 35, "French Guiana", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 36, "French Polynesia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 37, "Gambia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 38, "Germany", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 39, "Greece", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 40, "Greenland", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 41, "Holy See (Vatican City State)", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 42, "Hong Kong", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 43, "Hungary", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 44, "India", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 45, "Indonesia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 46, "Iran", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 47, "Iraq", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 48, "Israel", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 49, "Italy", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 50, "Japan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 51, "Kazakstan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 52, "Kenya", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 53, "Kuwait", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 54, "Latvia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 55, "Liechtenstein", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 56, "Lithuania", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 57, "Madagascar", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 58, "Malawi", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 59, "Malaysia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 60, "Mexico", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 61, "Moldova", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 62, "Morocco", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 63, "Mozambique", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 64, "Myanmar", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 65, "Nauru", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 66, "Nepal", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 67, "Netherlands", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 68, "New Zealand", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 69, "Nigeria", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 70, "North Korea", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 71, "Oman", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 72, "Pakistan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 73, "Paraguay", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 74, "Peru", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 75, "Philippines", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 76, "Poland", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 77, "Puerto Rico", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 78, "Romania", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 79, "Runion", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 80, "Russian Federation", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 81, "Saint Vincent and the Grenadines", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 82, "Saudi Arabia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 83, "Senegal", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 84, "Slovakia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 85, "South Africa", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 86, "South Korea", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 87, "Spain", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 88, "Sri Lanka", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 89, "Sudan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 90, "Sweden", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 91, "Switzerland", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 92, "Taiwan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 93, "Tanzania", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 94, "Thailand", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 95, "Tonga", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 96, "Tunisia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 97, "Turkey", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 98, "Turkmenistan", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 99, "Tuvalu", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 100, "Ukraine", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 101, "United Arab Emirates", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 102, "United Kingdom", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 103, "United States", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 104, "Venezuela", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 105, "Vietnam", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 106, "Virgin Islands, U.S.", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 107, "Yemen", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 108, "Yugoslavia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) },
                    { 109, "Zambia", new DateTime(2006, 2, 15, 9, 44, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "language",
                columns: new[] { "language_id", "last_update", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2006, 2, 15, 10, 2, 19, 0, DateTimeKind.Utc), "English             " },
                    { 2, new DateTime(2006, 2, 15, 10, 2, 19, 0, DateTimeKind.Utc), "Italian             " },
                    { 3, new DateTime(2006, 2, 15, 10, 2, 19, 0, DateTimeKind.Utc), "Japanese            " },
                    { 4, new DateTime(2006, 2, 15, 10, 2, 19, 0, DateTimeKind.Utc), "Mandarin            " },
                    { 5, new DateTime(2006, 2, 15, 10, 2, 19, 0, DateTimeKind.Utc), "French              " },
                    { 6, new DateTime(2006, 2, 15, 10, 2, 19, 0, DateTimeKind.Utc), "German              " }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "role-admin", "role-admin", "Admin", "ADMIN" },
                    { "role-customer", "role-customer", "Customer", "CUSTOMER" },
                    { "role-staff", "role-staff", "Staff", "STAFF" }
                });

            migrationBuilder.InsertData(
                table: "city",
                columns: new[] { "city_id", "city", "country_id", "last_update" },
                values: new object[,]
                {
                    { 1, "A Corua (La Corua)", (short)87, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 2, "Abha", (short)82, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 3, "Abu Dhabi", (short)101, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 4, "Acua", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 5, "Adana", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 6, "Addis Abeba", (short)31, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 7, "Aden", (short)107, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 8, "Adoni", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 9, "Ahmadnagar", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 10, "Akishima", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 11, "Akron", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 12, "al-Ayn", (short)101, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 13, "al-Hawiya", (short)82, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 14, "al-Manama", (short)11, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 15, "al-Qadarif", (short)89, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 16, "al-Qatif", (short)82, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 17, "Alessandria", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 18, "Allappuzha (Alleppey)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 19, "Allende", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 20, "Almirante Brown", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 21, "Alvorada", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 22, "Ambattur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 23, "Amersfoort", (short)67, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 24, "Amroha", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 25, "Angra dos Reis", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 26, "Anpolis", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 27, "Antofagasta", (short)22, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 28, "Aparecida de Goinia", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 29, "Apeldoorn", (short)67, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 30, "Araatuba", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 31, "Arak", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 32, "Arecibo", (short)77, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 33, "Arlington", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 34, "Ashdod", (short)48, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 35, "Ashgabat", (short)98, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 36, "Ashqelon", (short)48, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 37, "Asuncin", (short)73, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 38, "Athenai", (short)39, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 39, "Atinsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 40, "Atlixco", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 41, "Augusta-Richmond County", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 42, "Aurora", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 43, "Avellaneda", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 44, "Bag", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 45, "Baha Blanca", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 46, "Baicheng", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 47, "Baiyin", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 48, "Baku", (short)10, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 49, "Balaiha", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 50, "Balikesir", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 51, "Balurghat", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 52, "Bamenda", (short)19, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 53, "Bandar Seri Begawan", (short)16, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 54, "Banjul", (short)37, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 55, "Barcelona", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 56, "Basel", (short)91, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 57, "Bat Yam", (short)48, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 58, "Batman", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 59, "Batna", (short)2, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 60, "Battambang", (short)18, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 61, "Baybay", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 62, "Bayugan", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 63, "Bchar", (short)2, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 64, "Beira", (short)63, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 65, "Bellevue", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 66, "Belm", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 67, "Benguela", (short)4, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 68, "Beni-Mellal", (short)62, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 69, "Benin City", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 70, "Bergamo", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 71, "Berhampore (Baharampur)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 72, "Bern", (short)91, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 73, "Bhavnagar", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 74, "Bhilwara", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 75, "Bhimavaram", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 76, "Bhopal", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 77, "Bhusawal", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 78, "Bijapur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 79, "Bilbays", (short)29, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 80, "Binzhou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 81, "Birgunj", (short)66, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 82, "Bislig", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 83, "Blumenau", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 84, "Boa Vista", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 85, "Boksburg", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 86, "Botosani", (short)78, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 87, "Botshabelo", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 88, "Bradford", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 89, "Braslia", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 90, "Bratislava", (short)84, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 91, "Brescia", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 92, "Brest", (short)34, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 93, "Brindisi", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 94, "Brockton", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 95, "Bucuresti", (short)78, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 96, "Buenaventura", (short)24, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 97, "Bydgoszcz", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 98, "Cabuyao", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 99, "Callao", (short)74, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 100, "Cam Ranh", (short)105, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 101, "Cape Coral", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 102, "Caracas", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 103, "Carmen", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 104, "Cavite", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 105, "Cayenne", (short)35, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 106, "Celaya", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 107, "Chandrapur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 108, "Changhwa", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 109, "Changzhou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 110, "Chapra", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 111, "Charlotte Amalie", (short)106, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 112, "Chatsworth", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 113, "Cheju", (short)86, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 114, "Chiayi", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 115, "Chisinau", (short)61, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 116, "Chungho", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 117, "Cianjur", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 118, "Ciomas", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 119, "Ciparay", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 120, "Citrus Heights", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 121, "Citt del Vaticano", (short)41, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 122, "Ciudad del Este", (short)73, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 123, "Clarksville", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 124, "Coacalco de Berriozbal", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 125, "Coatzacoalcos", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 126, "Compton", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 127, "Coquimbo", (short)22, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 128, "Crdoba", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 129, "Cuauhtmoc", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 130, "Cuautla", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 131, "Cuernavaca", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 132, "Cuman", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 133, "Czestochowa", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 134, "Dadu", (short)72, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 135, "Dallas", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 136, "Datong", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 137, "Daugavpils", (short)54, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 138, "Davao", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 139, "Daxian", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 140, "Dayton", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 141, "Deba Habe", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 142, "Denizli", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 143, "Dhaka", (short)12, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 144, "Dhule (Dhulia)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 145, "Dongying", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 146, "Donostia-San Sebastin", (short)87, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 147, "Dos Quebradas", (short)24, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 148, "Duisburg", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 149, "Dundee", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 150, "Dzerzinsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 151, "Ede", (short)67, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 152, "Effon-Alaiye", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 153, "El Alto", (short)14, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 154, "El Fuerte", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 155, "El Monte", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 156, "Elista", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 157, "Emeishan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 158, "Emmen", (short)67, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 159, "Enshi", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 160, "Erlangen", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 161, "Escobar", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 162, "Esfahan", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 163, "Eskisehir", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 164, "Etawah", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 165, "Ezeiza", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 166, "Ezhou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 167, "Faaa", (short)36, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 168, "Fengshan", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 169, "Firozabad", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 170, "Florencia", (short)24, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 171, "Fontana", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 172, "Fukuyama", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 173, "Funafuti", (short)99, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 174, "Fuyu", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 175, "Fuzhou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 176, "Gandhinagar", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 177, "Garden Grove", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 178, "Garland", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 179, "Gatineau", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 180, "Gaziantep", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 181, "Gijn", (short)87, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 182, "Gingoog", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 183, "Goinia", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 184, "Gorontalo", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 185, "Grand Prairie", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 186, "Graz", (short)9, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 187, "Greensboro", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 188, "Guadalajara", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 189, "Guaruj", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 190, "guas Lindas de Gois", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 191, "Gulbarga", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 192, "Hagonoy", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 193, "Haining", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 194, "Haiphong", (short)105, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 195, "Haldia", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 196, "Halifax", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 197, "Halisahar", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 198, "Halle/Saale", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 199, "Hami", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 200, "Hamilton", (short)68, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 201, "Hanoi", (short)105, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 202, "Hidalgo", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 203, "Higashiosaka", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 204, "Hino", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 205, "Hiroshima", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 206, "Hodeida", (short)107, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 207, "Hohhot", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 208, "Hoshiarpur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 209, "Hsichuh", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 210, "Huaian", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 211, "Hubli-Dharwad", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 212, "Huejutla de Reyes", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 213, "Huixquilucan", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 214, "Hunuco", (short)74, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 215, "Ibirit", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 216, "Idfu", (short)29, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 217, "Ife", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 218, "Ikerre", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 219, "Iligan", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 220, "Ilorin", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 221, "Imus", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 222, "Inegl", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 223, "Ipoh", (short)59, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 224, "Isesaki", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 225, "Ivanovo", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 226, "Iwaki", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 227, "Iwakuni", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 228, "Iwatsuki", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 229, "Izumisano", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 230, "Jaffna", (short)88, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 231, "Jaipur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 232, "Jakarta", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 233, "Jalib al-Shuyukh", (short)53, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 234, "Jamalpur", (short)12, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 235, "Jaroslavl", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 236, "Jastrzebie-Zdrj", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 237, "Jedda", (short)82, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 238, "Jelets", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 239, "Jhansi", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 240, "Jinchang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 241, "Jining", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 242, "Jinzhou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 243, "Jodhpur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 244, "Johannesburg", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 245, "Joliet", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 246, "Jos Azueta", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 247, "Juazeiro do Norte", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 248, "Juiz de Fora", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 249, "Junan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 250, "Jurez", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 251, "Kabul", (short)1, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 252, "Kaduna", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 253, "Kakamigahara", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 254, "Kaliningrad", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 255, "Kalisz", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 256, "Kamakura", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 257, "Kamarhati", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 258, "Kamjanets-Podilskyi", (short)100, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 259, "Kamyin", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 260, "Kanazawa", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 261, "Kanchrapara", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 262, "Kansas City", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 263, "Karnal", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 264, "Katihar", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 265, "Kermanshah", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 266, "Kilis", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 267, "Kimberley", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 268, "Kimchon", (short)86, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 269, "Kingstown", (short)81, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 270, "Kirovo-Tepetsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 271, "Kisumu", (short)52, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 272, "Kitwe", (short)109, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 273, "Klerksdorp", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 274, "Kolpino", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 275, "Konotop", (short)100, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 276, "Koriyama", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 277, "Korla", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 278, "Korolev", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 279, "Kowloon and New Kowloon", (short)42, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 280, "Kragujevac", (short)108, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 281, "Ktahya", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 282, "Kuching", (short)59, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 283, "Kumbakonam", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 284, "Kurashiki", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 285, "Kurgan", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 286, "Kursk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 287, "Kuwana", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 288, "La Paz", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 289, "La Plata", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 290, "La Romana", (short)27, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 291, "Laiwu", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 292, "Lancaster", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 293, "Laohekou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 294, "Lapu-Lapu", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 295, "Laredo", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 296, "Lausanne", (short)91, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 297, "Le Mans", (short)34, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 298, "Lengshuijiang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 299, "Leshan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 300, "Lethbridge", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 301, "Lhokseumawe", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 302, "Liaocheng", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 303, "Liepaja", (short)54, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 304, "Lilongwe", (short)58, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 305, "Lima", (short)74, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 306, "Lincoln", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 307, "Linz", (short)9, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 308, "Lipetsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 309, "Livorno", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 310, "Ljubertsy", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 311, "Loja", (short)28, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 312, "London", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 313, "London", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 314, "Lublin", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 315, "Lubumbashi", (short)25, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 316, "Lungtan", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 317, "Luzinia", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 318, "Madiun", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 319, "Mahajanga", (short)57, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 320, "Maikop", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 321, "Malm", (short)90, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 322, "Manchester", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 323, "Mandaluyong", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 324, "Mandi Bahauddin", (short)72, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 325, "Mannheim", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 326, "Maracabo", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 327, "Mardan", (short)72, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 328, "Maring", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 329, "Masqat", (short)71, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 330, "Matamoros", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 331, "Matsue", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 332, "Meixian", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 333, "Memphis", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 334, "Merlo", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 335, "Mexicali", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 336, "Miraj", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 337, "Mit Ghamr", (short)29, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 338, "Miyakonojo", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 339, "Mogiljov", (short)13, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 340, "Molodetno", (short)13, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 341, "Monclova", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 342, "Monywa", (short)64, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 343, "Moscow", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 344, "Mosul", (short)47, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 345, "Mukateve", (short)100, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 346, "Munger (Monghyr)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 347, "Mwanza", (short)93, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 348, "Mwene-Ditu", (short)25, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 349, "Myingyan", (short)64, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 350, "Mysore", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 351, "Naala-Porto", (short)63, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 352, "Nabereznyje Telny", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 353, "Nador", (short)62, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 354, "Nagaon", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 355, "Nagareyama", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 356, "Najafabad", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 357, "Naju", (short)86, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 358, "Nakhon Sawan", (short)94, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 359, "Nam Dinh", (short)105, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 360, "Namibe", (short)4, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 361, "Nantou", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 362, "Nanyang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 363, "NDjamna", (short)21, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 364, "Newcastle", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 365, "Nezahualcyotl", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 366, "Nha Trang", (short)105, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 367, "Niznekamsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 368, "Novi Sad", (short)108, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 369, "Novoterkassk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 370, "Nukualofa", (short)95, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 371, "Nuuk", (short)40, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 372, "Nyeri", (short)52, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 373, "Ocumare del Tuy", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 374, "Ogbomosho", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 375, "Okara", (short)72, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 376, "Okayama", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 377, "Okinawa", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 378, "Olomouc", (short)26, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 379, "Omdurman", (short)89, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 380, "Omiya", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 381, "Ondo", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 382, "Onomichi", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 383, "Oshawa", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 384, "Osmaniye", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 385, "ostka", (short)100, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 386, "Otsu", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 387, "Oulu", (short)33, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 388, "Ourense (Orense)", (short)87, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 389, "Owo", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 390, "Oyo", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 391, "Ozamis", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 392, "Paarl", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 393, "Pachuca de Soto", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 394, "Pak Kret", (short)94, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 395, "Palghat (Palakkad)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 396, "Pangkal Pinang", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 397, "Papeete", (short)36, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 398, "Parbhani", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 399, "Pathankot", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 400, "Patiala", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 401, "Patras", (short)39, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 402, "Pavlodar", (short)51, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 403, "Pemalang", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 404, "Peoria", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 405, "Pereira", (short)24, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 406, "Phnom Penh", (short)18, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 407, "Pingxiang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 408, "Pjatigorsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 409, "Plock", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 410, "Po", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 411, "Ponce", (short)77, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 412, "Pontianak", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 413, "Poos de Caldas", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 414, "Portoviejo", (short)28, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 415, "Probolinggo", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 416, "Pudukkottai", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 417, "Pune", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 418, "Purnea (Purnia)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 419, "Purwakarta", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 420, "Pyongyang", (short)70, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 421, "Qalyub", (short)29, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 422, "Qinhuangdao", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 423, "Qomsheh", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 424, "Quilmes", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 425, "Rae Bareli", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 426, "Rajkot", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 427, "Rampur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 428, "Rancagua", (short)22, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 429, "Ranchi", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 430, "Richmond Hill", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 431, "Rio Claro", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 432, "Rizhao", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 433, "Roanoke", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 434, "Robamba", (short)28, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 435, "Rockford", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 436, "Ruse", (short)17, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 437, "Rustenburg", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 438, "s-Hertogenbosch", (short)67, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 439, "Saarbrcken", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 440, "Sagamihara", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 441, "Saint Louis", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 442, "Saint-Denis", (short)79, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 443, "Sal", (short)62, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 444, "Salala", (short)71, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 445, "Salamanca", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 446, "Salinas", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 447, "Salzburg", (short)9, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 448, "Sambhal", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 449, "San Bernardino", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 450, "San Felipe de Puerto Plata", (short)27, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 451, "San Felipe del Progreso", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 452, "San Juan Bautista Tuxtepec", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 453, "San Lorenzo", (short)73, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 454, "San Miguel de Tucumn", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 455, "Sanaa", (short)107, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 456, "Santa Brbara dOeste", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 457, "Santa F", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 458, "Santa Rosa", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 459, "Santiago de Compostela", (short)87, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 460, "Santiago de los Caballeros", (short)27, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 461, "Santo Andr", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 462, "Sanya", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 463, "Sasebo", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 464, "Satna", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 465, "Sawhaj", (short)29, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 466, "Serpuhov", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 467, "Shahr-e Kord", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 468, "Shanwei", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 469, "Shaoguan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 470, "Sharja", (short)101, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 471, "Shenzhen", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 472, "Shikarpur", (short)72, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 473, "Shimoga", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 474, "Shimonoseki", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 475, "Shivapuri", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 476, "Shubra al-Khayma", (short)29, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 477, "Siegen", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 478, "Siliguri (Shiliguri)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 479, "Simferopol", (short)100, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 480, "Sincelejo", (short)24, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 481, "Sirjan", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 482, "Sivas", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 483, "Skikda", (short)2, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 484, "Smolensk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 485, "So Bernardo do Campo", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 486, "So Leopoldo", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 487, "Sogamoso", (short)24, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 488, "Sokoto", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 489, "Songkhla", (short)94, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 490, "Sorocaba", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 491, "Soshanguve", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 492, "Sousse", (short)96, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 493, "South Hill", (short)5, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 494, "Southampton", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 495, "Southend-on-Sea", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 496, "Southport", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 497, "Springs", (short)85, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 498, "Stara Zagora", (short)17, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 499, "Sterling Heights", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 500, "Stockport", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 501, "Sucre", (short)14, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 502, "Suihua", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 503, "Sullana", (short)74, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 504, "Sultanbeyli", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 505, "Sumqayit", (short)10, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 506, "Sumy", (short)100, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 507, "Sungai Petani", (short)59, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 508, "Sunnyvale", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 509, "Surakarta", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 510, "Syktyvkar", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 511, "Syrakusa", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 512, "Szkesfehrvr", (short)43, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 513, "Tabora", (short)93, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 514, "Tabriz", (short)46, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 515, "Tabuk", (short)82, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 516, "Tafuna", (short)3, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 517, "Taguig", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 518, "Taizz", (short)107, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 519, "Talavera", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 520, "Tallahassee", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 521, "Tama", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 522, "Tambaram", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 523, "Tanauan", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 524, "Tandil", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 525, "Tangail", (short)12, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 526, "Tanshui", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 527, "Tanza", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 528, "Tarlac", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 529, "Tarsus", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 530, "Tartu", (short)30, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 531, "Teboksary", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 532, "Tegal", (short)45, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 533, "Tel Aviv-Jaffa", (short)48, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 534, "Tete", (short)63, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 535, "Tianjin", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 536, "Tiefa", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 537, "Tieli", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 538, "Tokat", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 539, "Tonghae", (short)86, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 540, "Tongliao", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 541, "Torren", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 542, "Touliu", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 543, "Toulon", (short)34, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 544, "Toulouse", (short)34, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 545, "Trshavn", (short)32, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 546, "Tsaotun", (short)92, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 547, "Tsuyama", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 548, "Tuguegarao", (short)75, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 549, "Tychy", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 550, "Udaipur", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 551, "Udine", (short)49, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 552, "Ueda", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 553, "Uijongbu", (short)86, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 554, "Uluberia", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 555, "Urawa", (short)50, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 556, "Uruapan", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 557, "Usak", (short)97, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 558, "Usolje-Sibirskoje", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 559, "Uttarpara-Kotrung", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 560, "Vaduz", (short)55, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 561, "Valencia", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 562, "Valle de la Pascua", (short)104, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 563, "Valle de Santiago", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 564, "Valparai", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 565, "Vancouver", (short)20, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 566, "Varanasi (Benares)", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 567, "Vicente Lpez", (short)6, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 568, "Vijayawada", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 569, "Vila Velha", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 570, "Vilnius", (short)56, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 571, "Vinh", (short)105, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 572, "Vitria de Santo Anto", (short)15, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 573, "Warren", (short)103, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 574, "Weifang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 575, "Witten", (short)38, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 576, "Woodridge", (short)8, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 577, "Wroclaw", (short)76, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 578, "Xiangfan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 579, "Xiangtan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 580, "Xintai", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 581, "Xinxiang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 582, "Yamuna Nagar", (short)44, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 583, "Yangor", (short)65, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 584, "Yantai", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 585, "Yaound", (short)19, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 586, "Yerevan", (short)7, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 587, "Yinchuan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 588, "Yingkou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 589, "York", (short)102, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 590, "Yuncheng", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 591, "Yuzhou", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 592, "Zalantun", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 593, "Zanzibar", (short)93, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 594, "Zaoyang", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 595, "Zapopan", (short)60, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 596, "Zaria", (short)69, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 597, "Zeleznogorsk", (short)80, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 598, "Zhezqazghan", (short)51, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 599, "Zhoushan", (short)23, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) },
                    { 600, "Ziguinchor", (short)83, new DateTime(2006, 2, 15, 9, 45, 25, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_address_city_id",
                table: "address",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_city_country_id",
                table: "city",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_address_id",
                table: "customer",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_store_id",
                table: "customer",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_film_language_id",
                table: "film",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_film_original_language_id",
                table: "film",
                column: "original_language_id");

            migrationBuilder.CreateIndex(
                name: "IX_film_actor_film_id",
                table: "film_actor",
                column: "film_id");

            migrationBuilder.CreateIndex(
                name: "IX_film_category_category_id",
                table: "film_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_film_id",
                table: "inventory",
                column: "film_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_store_id",
                table: "inventory",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_customer_id",
                table: "payment",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_rental_id",
                table: "payment",
                column: "rental_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_staff_id",
                table: "payment",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family",
                schema: "identity",
                table: "refresh_tokens",
                column: "family");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token",
                schema: "identity",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                schema: "identity",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_customer_id",
                table: "rental",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_inventory_id",
                table: "rental",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_staff_id",
                table: "rental",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_RoleId",
                schema: "identity",
                table: "role_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identity",
                table: "roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_address_id",
                table: "staff",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_store_id",
                table: "staff",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_store_address_id",
                table: "store",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_store_manager_staff_id",
                table: "store",
                column: "manager_staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_UserId",
                schema: "identity",
                table: "user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_logins_UserId",
                schema: "identity",
                table: "user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                schema: "identity",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identity",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_customer_id",
                schema: "identity",
                table: "users",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_customer_store_store_id",
                table: "customer",
                column: "store_id",
                principalTable: "store",
                principalColumn: "store_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_store_store_id",
                table: "inventory",
                column: "store_id",
                principalTable: "store",
                principalColumn: "store_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_rental_rental_id",
                table: "payment",
                column: "rental_id",
                principalTable: "rental",
                principalColumn: "rental_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_staff_staff_id",
                table: "payment",
                column: "staff_id",
                principalTable: "staff",
                principalColumn: "staff_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rental_staff_staff_id",
                table: "rental",
                column: "staff_id",
                principalTable: "staff",
                principalColumn: "staff_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_staff_store_store_id",
                table: "staff",
                column: "store_id",
                principalTable: "store",
                principalColumn: "store_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_address_city_city_id",
                table: "address");

            migrationBuilder.DropForeignKey(
                name: "FK_staff_address_address_id",
                table: "staff");

            migrationBuilder.DropForeignKey(
                name: "FK_store_address_address_id",
                table: "store");

            migrationBuilder.DropForeignKey(
                name: "FK_staff_store_store_id",
                table: "staff");

            migrationBuilder.DropTable(
                name: "film_actor");

            migrationBuilder.DropTable(
                name: "film_category");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "role_claims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_claims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_logins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "actor");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "rental");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "inventory");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "film");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropTable(
                name: "city");

            migrationBuilder.DropTable(
                name: "country");

            migrationBuilder.DropTable(
                name: "address");

            migrationBuilder.DropTable(
                name: "store");

            migrationBuilder.DropTable(
                name: "staff");
        }
    }
}
