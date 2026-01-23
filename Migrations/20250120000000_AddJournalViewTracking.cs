using Microsoft.EntityFrameworkCore.Migrations;

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalViewTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JournalViews",
                columns: table => new
                {
                    JournalViewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalViews", x => x.JournalViewId);
                    table.ForeignKey(
                        name: "FK_JournalViews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalViews_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalViews_JournalId",
                table: "JournalViews",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalViews_JournalId_UserId",
                table: "JournalViews",
                columns: new[] { "JournalId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalViews_UserId",
                table: "JournalViews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalViews");
        }
    }
}
