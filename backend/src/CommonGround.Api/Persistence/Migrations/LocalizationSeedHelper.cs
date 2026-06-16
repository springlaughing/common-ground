using Microsoft.EntityFrameworkCore.Migrations;

namespace CommonGround.Api.Persistence.Migrations;

/// <summary>
/// Seeds German ("de") translation rows for the localized content tables. The English
/// text stays canonical on the base entities (the field-level fallback); only the
/// non-default locale lives here. Translation rows attach to the same immutable
/// question/option/group/insight IDs produced by <see cref="SeedDataHelper"/>, so
/// scoring (which reads IDs/weights) is untouched.
///
/// Content comes from <see cref="LocalizationSeedData"/> (a generated C# file produced
/// from the authored source files by <c>scripts/generate-localization-seed.mjs</c>).
///
/// NOTE on the two kinds of "title": this helper seeds <b>dimension-group</b> titles
/// (the 10 section headings → <c>DimensionGroupTranslations</c>). The 76 per-dimension
/// titles (<c>DimensionTitles</c>, the per-insight headers) are seeded separately in
/// US3 / T035 via <see cref="SeedDimensionTitlesUp"/>.
/// </summary>
internal static class LocalizationSeedHelper
{
    private const string Locale = "de";

    // ── US1 / T023: questions, options, group titles, insight texts ───────────

    internal static void SeedUp(MigrationBuilder mb)
    {
        InsertQuestionTranslations(mb);
        InsertAnswerOptionTranslations(mb);
        InsertDimensionGroupTranslations(mb);
        InsertInsightSnippetTranslations(mb);
    }

    internal static void SeedDown(MigrationBuilder mb)
    {
        DeleteLocale(mb, "QuestionTranslations");
        DeleteLocale(mb, "AnswerOptionTranslations");
        DeleteLocale(mb, "DimensionGroupTranslations");
        DeleteLocale(mb, "InsightSnippetTranslations");
    }

    private static void InsertQuestionTranslations(MigrationBuilder mb)
    {
        var rows = string.Join(",\n", LocalizationSeedData.Questions.Select(q =>
            $"('{SeedDataHelper.G($"qt.{Locale}.s{q.Section}q{q.Q}")}'," +
            $"'{SeedDataHelper.Q(q.Section, q.Q)}','{Locale}',{Esc(q.Text)})"));
        mb.Sql($"""
            INSERT INTO "QuestionTranslations" ("Id","QuestionId","Locale","Text")
            VALUES {rows}
            """);
    }

    private static void InsertAnswerOptionTranslations(MigrationBuilder mb)
    {
        var rows = string.Join(",\n", LocalizationSeedData.Questions.SelectMany(q => q.Options.Select(o =>
            $"('{SeedDataHelper.G($"aot.{Locale}.s{q.Section}q{q.Q}.{o.Letter}")}'," +
            $"'{SeedDataHelper.AO(q.Section, q.Q, o.Letter[0])}','{Locale}',{Esc(o.Text)})")));
        mb.Sql($"""
            INSERT INTO "AnswerOptionTranslations" ("Id","AnswerOptionId","Locale","Text")
            VALUES {rows}
            """);
    }

    /// <summary>Seeds the 10 dimension-<b>group</b> titles (section headings).</summary>
    private static void InsertDimensionGroupTranslations(MigrationBuilder mb)
    {
        var rows = string.Join(",\n", LocalizationSeedData.GroupTitles.Select(g =>
            $"('{SeedDataHelper.G($"dgt.{Locale}.{g.GroupId}")}'," +
            $"'{SeedDataHelper.DG(g.GroupId)}','{Locale}',{Esc(g.Title)})"));
        mb.Sql($"""
            INSERT INTO "DimensionGroupTranslations" ("Id","DimensionGroupId","Locale","Title")
            VALUES {rows}
            """);
    }

    private static void InsertInsightSnippetTranslations(MigrationBuilder mb)
    {
        var rows = string.Join(",\n", LocalizationSeedData.Insights.Select(i =>
            $"('{SeedDataHelper.G($"ist.{Locale}.{i.Dim}")}'," +
            $"'{SeedDataHelper.IS(i.Dim)}','{Locale}',{Esc(i.Text)})"));
        mb.Sql($"""
            INSERT INTO "InsightSnippetTranslations" ("Id","InsightSnippetId","Locale","Text")
            VALUES {rows}
            """);
    }

    // ── US3 / T035: the 76 per-dimension titles (locale-first, en + de) ───────

    /// <summary>
    /// Seeds <c>DimensionTitles</c> — the short per-insight headers. Locale-first:
    /// one row per (DimensionId, Locale) for <b>both</b> en and de. Called by the
    /// US3 migration, not the US1 one.
    /// </summary>
    internal static void SeedDimensionTitlesUp(MigrationBuilder mb)
    {
        var rows = LocalizationSeedData.DimensionTitles.SelectMany(d => new[]
        {
            (Locale: "en", Title: d.En, d.Dim),
            (Locale: "de", Title: d.De, d.Dim),
        }).Select(t =>
            $"('{SeedDataHelper.G($"dt.{t.Locale}.{t.Dim}")}'," +
            $"{Esc(t.Dim)},'{t.Locale}',{Esc(t.Title)})");
        mb.Sql($"""
            INSERT INTO "DimensionTitles" ("Id","DimensionId","Locale","Title")
            VALUES {string.Join(",\n", rows)}
            """);
    }

    internal static void SeedDimensionTitlesDown(MigrationBuilder mb) =>
        mb.Sql("""DELETE FROM "DimensionTitles" """);

    // ── Polish (post-MVP): re-sync reworded de question texts ─────────────────
    // Four S9 questions were reworded after the MVP shipped (gender-neutral phrasing
    // and a clearer S9Q3). The de rows already exist in prod from SeedUp, so this is
    // an in-place UPDATE rather than an insert. Up pulls the current generated text
    // (single source of truth = LocalizationSeedData); Down restores the MVP wording.

    private static readonly (int Section, int Q)[] RewordedQuestions =
        { (9, 1), (9, 3), (9, 4), (9, 6) };

    internal static void UpdateRewordedQuestionsUp(MigrationBuilder mb)
    {
        foreach (var (section, q) in RewordedQuestions)
        {
            var seed = LocalizationSeedData.Questions.Single(x => x.Section == section && x.Q == q);
            UpdateQuestionText(mb, section, q, seed.Text);
        }
    }

    internal static void UpdateRewordedQuestionsDown(MigrationBuilder mb)
    {
        UpdateQuestionText(mb, 9, 1,
            "Du bemerkst Spannung zwischen dir und einer Kollegin oder einem Kollegen. Was ist dir zuerst am wichtigsten?");
        UpdateQuestionText(mb, 9, 3,
            "Wenn ein schwieriges Gespräch nötig ist, was soll es vor allem hervorbringen?");
        UpdateQuestionText(mb, 9, 4,
            "Wenn sich Spannung oder Konflikt zwischen dir und einer Kollegin oder einem Kollegen nicht von selbst löst, würdest du dir jemanden zur Moderation wünschen, und wer sollte das sein?");
        UpdateQuestionText(mb, 9, 6,
            "Du bist in einer Führungsrolle, und ein Teammitglied kommt zu dir und sagt, es sei in einem Konflikt mit einer Kollegin oder einem Kollegen. Was ist dein Instinkt?");
    }

    private static void UpdateQuestionText(MigrationBuilder mb, int section, int q, string text) =>
        mb.Sql($"""
            UPDATE "QuestionTranslations" SET "Text" = {Esc(text)}
            WHERE "Id" = '{SeedDataHelper.G($"qt.{Locale}.s{section}q{q}")}'
            """);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void DeleteLocale(MigrationBuilder mb, string table) =>
        mb.Sql($"""DELETE FROM "{table}" WHERE "Locale" = '{Locale}'""");

    private static string Esc(string s) => $"'{s.Replace("'", "''")}'";
}
