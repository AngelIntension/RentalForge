using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalForge.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffIdToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "staff_id",
                schema: "identity",
                table: "users",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_staff_id",
                schema: "identity",
                table: "users",
                column: "staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_staff_staff_id",
                schema: "identity",
                table: "users",
                column: "staff_id",
                principalTable: "staff",
                principalColumn: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_staff_staff_id",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_staff_id",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "staff_id",
                schema: "identity",
                table: "users");
        }
    }
}
