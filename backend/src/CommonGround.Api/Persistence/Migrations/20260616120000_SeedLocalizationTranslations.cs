using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonGround.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedLocalizationTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            LocalizationSeedHelper.SeedUp(migrationBuilder);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            LocalizationSeedHelper.SeedDown(migrationBuilder);
    }
}
