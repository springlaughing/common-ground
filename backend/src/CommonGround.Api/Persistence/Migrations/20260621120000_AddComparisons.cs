using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonGround.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ComparisonSessions/ComparisonParticipants tables already exist (InitialSchema).
            // This migration adds the Invite table, the per-participant DisplayLabel, and the
            // lookup indexes the comparison flow needs.
            migrationBuilder.AddColumn<string>(
                name: "DisplayLabel",
                table: "ComparisonParticipants",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Invites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComparisonSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviterResponseSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviterLabel = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invites_ComparisonSessions_ComparisonSessionId",
                        column: x => x.ComparisonSessionId,
                        principalTable: "ComparisonSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonParticipants_ResponseSetId",
                table: "ComparisonParticipants",
                column: "ResponseSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonParticipants_ComparisonSessionId_ResponseSetId",
                table: "ComparisonParticipants",
                columns: new[] { "ComparisonSessionId", "ResponseSetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invites_ComparisonSessionId",
                table: "Invites",
                column: "ComparisonSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Invites_TokenHash",
                table: "Invites",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invites");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonParticipants_ResponseSetId",
                table: "ComparisonParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ComparisonParticipants_ComparisonSessionId_ResponseSetId",
                table: "ComparisonParticipants");

            migrationBuilder.DropColumn(
                name: "DisplayLabel",
                table: "ComparisonParticipants");
        }
    }
}
