using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalForge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalLanguageIdToFilm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE film
                    ADD COLUMN original_language_id smallint;

                ALTER TABLE film
                    ADD CONSTRAINT fk_film_original_language
                    FOREIGN KEY (original_language_id)
                    REFERENCES language(language_id)
                    ON UPDATE CASCADE
                    ON DELETE RESTRICT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE film
                    DROP CONSTRAINT IF EXISTS fk_film_original_language;

                ALTER TABLE film
                    DROP COLUMN IF EXISTS original_language_id;
                """);
        }
    }
}
