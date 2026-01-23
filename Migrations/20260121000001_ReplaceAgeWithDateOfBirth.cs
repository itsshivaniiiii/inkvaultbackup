using Microsoft.EntityFrameworkCore.Migrations;

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAgeWithDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the Age column if it exists
            migrationBuilder.DropColumn(
                name: "Age",
                table: "AspNetUsers");

            // Add DateOfBirth column
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            // Add LastBirthdayEmailSent column for tracking birthday emails
            migrationBuilder.AddColumn<DateTime>(
                name: "LastBirthdayEmailSent",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop DateOfBirth and LastBirthdayEmailSent columns
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastBirthdayEmailSent",
                table: "AspNetUsers");

            // Re-add Age column
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }
    }
}
