using CommonGround.Modules.Reporting;
using CommonGround.SharedKernel.Interfaces;
using FluentAssertions;
using Moq;

namespace CommonGround.UnitTests.Reporting;

public sealed class ScoringEngineTests
{
    private static (ScoringEngine Engine, Mock<IQuestionnaireReader> Reader) CreateEngine(
        ReportingTestContext db)
    {
        var reader = new Mock<IQuestionnaireReader>();
        return (new ScoringEngine(reader.Object, db), reader);
    }

    [Fact]
    public async Task Primary_Weights_Accumulate_At_Full_Weight()
    {
        using var db = ReportingTestContext.Create();
        var (engine, reader) = CreateEngine(db);
        var optionId = Guid.NewGuid();
        var responseSetId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        reader.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(optionId, "dim_a", 5)]);

        reader.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["dim_a"] = 5m });

        var scores = await engine.ScoreAsync(responseSetId, versionId,
            [new(optionId, null)]);

        scores.Should().ContainSingle(s => s.DimensionId == "dim_a")
            .Which.Should().Match<Modules.Reporting.Entities.DimensionScore>(
                s => s.RawScore == 5m && s.NormalisedScore == 1.0m);
    }

    [Fact]
    public async Task Secondary_Weights_Accumulate_At_Half_Weight()
    {
        using var db = ReportingTestContext.Create();
        var (engine, reader) = CreateEngine(db);
        var primaryId = Guid.NewGuid();
        var secondaryId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        reader.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new(primaryId, "dim_a", 4),
                new(secondaryId, "dim_a", 4),
            ]);

        reader.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["dim_a"] = 10m });

        // primary 4 × 1.0 = 4, secondary 4 × 0.5 = 2  →  raw = 6
        var scores = await engine.ScoreAsync(Guid.NewGuid(), versionId,
            [new(primaryId, secondaryId)]);

        scores.Should().ContainSingle(s => s.DimensionId == "dim_a")
            .Which.RawScore.Should().Be(6.0m);
    }

    [Fact]
    public async Task Negative_Weights_Reduce_Score()
    {
        using var db = ReportingTestContext.Create();
        var (engine, reader) = CreateEngine(db);
        var optionA = Guid.NewGuid();
        var optionB = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        reader.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new(optionA, "dim_a", 5),
                new(optionB, "dim_a", -3),
            ]);

        reader.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["dim_a"] = 5m });

        // 5 + (-3) = 2 raw
        var scores = await engine.ScoreAsync(Guid.NewGuid(), versionId,
            [new(optionA, null), new(optionB, null)]);

        scores.Should().ContainSingle(s => s.DimensionId == "dim_a")
            .Which.RawScore.Should().Be(2.0m);
    }

    [Fact]
    public async Task Score_Above_Max_Is_Clamped_To_One()
    {
        using var db = ReportingTestContext.Create();
        var (engine, reader) = CreateEngine(db);
        var optionId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        reader.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(optionId, "dim_a", 10)]);

        reader.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["dim_a"] = 5m });

        var scores = await engine.ScoreAsync(Guid.NewGuid(), versionId,
            [new(optionId, null)]);

        scores.Should().ContainSingle(s => s.DimensionId == "dim_a")
            .Which.NormalisedScore.Should().Be(1.0m);
    }

    [Fact]
    public async Task Score_Below_Zero_Is_Clamped_To_Zero()
    {
        using var db = ReportingTestContext.Create();
        var (engine, reader) = CreateEngine(db);
        var optionId = Guid.NewGuid();
        var versionId = Guid.NewGuid();

        reader.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(optionId, "dim_a", -5)]);

        reader.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, decimal> { ["dim_a"] = 5m });

        var scores = await engine.ScoreAsync(Guid.NewGuid(), versionId,
            [new(optionId, null)]);

        scores.Should().ContainSingle(s => s.DimensionId == "dim_a")
            .Which.NormalisedScore.Should().Be(0.0m);
    }

    [Fact]
    public async Task Same_Inputs_Produce_Same_Normalised_Scores()
    {
        using var db1 = ReportingTestContext.Create();
        using var db2 = ReportingTestContext.Create();
        var optionId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var inputs = new List<ScoringInput> { new(optionId, null) };

        var weights = new List<DimensionWeightDto> { new(optionId, "dim_a", 7) };
        var maxScores = new Dictionary<string, decimal> { ["dim_a"] = 10m };

        var (engine1, reader1) = CreateEngine(db1);
        reader1.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(weights);
        reader1.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxScores);

        var (engine2, reader2) = CreateEngine(db2);
        reader2.Setup(r => r.GetDimensionWeightsForOptionsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(weights);
        reader2.Setup(r => r.GetDimensionMaxScoresAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maxScores);

        var result1 = await engine1.ScoreAsync(Guid.NewGuid(), versionId, inputs);
        var result2 = await engine2.ScoreAsync(Guid.NewGuid(), versionId, inputs);

        result1.Single(s => s.DimensionId == "dim_a").NormalisedScore
            .Should().Be(result2.Single(s => s.DimensionId == "dim_a").NormalisedScore);
    }
}
