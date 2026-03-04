using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class AddHasCompletedFirstLoginFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedFirstLogin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCompletedFirstLogin",
                table: "AspNetUsers");
        }
    }
}
