using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAnonymousToJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "Journals",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Journals");
        }
    }
}
