using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonGround.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResponseSetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComparisonSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DimensionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DimensionScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RawScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    NormalisedScore = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsightSnippets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsightSnippets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateResultTokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AccessCodeHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComparisonSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonParticipants_ComparisonSessions_ComparisonSession~",
                        column: x => x.ComparisonSessionId,
                        principalTable: "ComparisonSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DimensionGroupMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionGroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DimensionGroupMemberships_DimensionGroups_DimensionGroupId",
                        column: x => x.DimensionGroupId,
                        principalTable: "DimensionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DimensionMaxScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionMaxScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DimensionMaxScores_QuestionnaireVersions_QuestionnaireVersi~",
                        column: x => x.QuestionnaireVersionId,
                        principalTable: "QuestionnaireVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SectionIndex = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_QuestionnaireVersions_QuestionnaireVersionId",
                        column: x => x.QuestionnaireVersionId,
                        principalTable: "QuestionnaireVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryAnswerOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondaryAnswerOptionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_ResponseSets_ResponseSetId",
                        column: x => x.ResponseSetId,
                        principalTable: "ResponseSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DimensionWeights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DimensionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimensionWeights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DimensionWeights_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_ResponseSetId_QuestionId",
                table: "Answers",
                columns: new[] { "ResponseSetId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonParticipants_ComparisonSessionId",
                table: "ComparisonParticipants",
                column: "ComparisonSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionGroupMemberships_DimensionGroupId",
                table: "DimensionGroupMemberships",
                column: "DimensionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DimensionGroups_GroupId",
                table: "DimensionGroups",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionMaxScores_QuestionnaireVersionId_DimensionId",
                table: "DimensionMaxScores",
                columns: new[] { "QuestionnaireVersionId", "DimensionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionScores_ResponseSetId_DimensionId",
                table: "DimensionScores",
                columns: new[] { "ResponseSetId", "DimensionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DimensionWeights_AnswerOptionId_DimensionId",
                table: "DimensionWeights",
                columns: new[] { "AnswerOptionId", "DimensionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionnaireVersionId",
                table: "Questions",
                column: "QuestionnaireVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "ComparisonParticipants");

            migrationBuilder.DropTable(
                name: "DimensionGroupMemberships");

            migrationBuilder.DropTable(
                name: "DimensionMaxScores");

            migrationBuilder.DropTable(
                name: "DimensionScores");

            migrationBuilder.DropTable(
                name: "DimensionWeights");

            migrationBuilder.DropTable(
                name: "InsightSnippets");

            migrationBuilder.DropTable(
                name: "ResponseSets");

            migrationBuilder.DropTable(
                name: "ComparisonSessions");

            migrationBuilder.DropTable(
                name: "DimensionGroups");

            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionnaireVersions");
        }
    }
}
