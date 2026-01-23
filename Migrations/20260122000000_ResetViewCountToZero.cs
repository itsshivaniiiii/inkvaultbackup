using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class ResetViewCountToZero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear all existing journal views
            migrationBuilder.Sql("DELETE FROM JournalViews");
            
            // Reset all view counts to 0
            migrationBuilder.Sql("UPDATE Journals SET ViewCount = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: This migration clears data, so Down cannot fully restore it
            // This is a data-reset migration
        }
    }
}
