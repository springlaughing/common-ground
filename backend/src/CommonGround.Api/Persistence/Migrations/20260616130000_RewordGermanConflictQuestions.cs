using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonGround.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RewordGermanConflictQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            LocalizationSeedHelper.UpdateRewordedQuestionsUp(migrationBuilder);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            LocalizationSeedHelper.UpdateRewordedQuestionsDown(migrationBuilder);
    }
}
