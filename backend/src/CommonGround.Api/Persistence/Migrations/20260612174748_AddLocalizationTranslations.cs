using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonGround.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerOptionTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerOptionTranslations_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DimensionGroupTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionGroupTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DimensionGroupTranslations_DimensionGroups_DimensionGroupId",
                        column: x => x.DimensionGroupId,
                        principalTable: "DimensionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DimensionTitles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionTitles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsightSnippetTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsightSnippetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsightSnippetTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsightSnippetTranslations_InsightSnippets_InsightSnippetId",
                        column: x => x.InsightSnippetId,
                        principalTable: "InsightSnippets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Locale = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionTranslations_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptionTranslations_AnswerOptionId_Locale",
                table: "AnswerOptionTranslations",
                columns: new[] { "AnswerOptionId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionGroupTranslations_DimensionGroupId_Locale",
                table: "DimensionGroupTranslations",
                columns: new[] { "DimensionGroupId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionTitles_DimensionId_Locale",
                table: "DimensionTitles",
                columns: new[] { "DimensionId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsightSnippetTranslations_InsightSnippetId_Locale",
                table: "InsightSnippetTranslations",
                columns: new[] { "InsightSnippetId", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionTranslations_QuestionId_Locale",
                table: "QuestionTranslations",
                columns: new[] { "QuestionId", "Locale" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOptionTranslations");

            migrationBuilder.DropTable(
                name: "DimensionGroupTranslations");

            migrationBuilder.DropTable(
                name: "DimensionTitles");

            migrationBuilder.DropTable(
                name: "InsightSnippetTranslations");

            migrationBuilder.DropTable(
                name: "QuestionTranslations");
        }
    }
}
