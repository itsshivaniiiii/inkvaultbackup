using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class AddAbstractAndFullTextRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Abstract",
                table: "Journals",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FullTextRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalId = table.Column<int>(type: "integer", nullable: false),
                    RequesterId = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResponded = table.Column<bool>(type: "boolean", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullTextRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_FullTextRequests_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FullTextRequests_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FullTextRequests_JournalId_RequesterId",
                table: "FullTextRequests",
                columns: new[] { "JournalId", "RequesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FullTextRequests_RequesterId",
                table: "FullTextRequests",
                column: "RequesterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FullTextRequests");

            migrationBuilder.DropColumn(
                name: "Abstract",
                table: "Journals");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Journals",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
