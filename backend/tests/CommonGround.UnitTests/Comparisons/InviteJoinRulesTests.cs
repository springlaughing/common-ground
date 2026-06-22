using CommonGround.Modules.Comparisons.Entities;
using CommonGround.Modules.Comparisons.Services;
using FluentAssertions;

namespace CommonGround.UnitTests.Comparisons;

/// <summary>
/// T021 (US2) — the single-use + time-limited join rule: only an Active invite within its window
/// (plus the short grace period) may be joined. Used/Expired never re-open; the grace boundary is exact.
/// </summary>
public sealed class InviteJoinRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Active_WellWithinWindow_IsJoinable()
    {
        InviteJoinRules.IsJoinable(InviteStatus.Active, Now.AddDays(3), Now).Should().BeTrue();
    }

    [Fact]
    public void Active_JustExpired_ButWithinGrace_IsJoinable()
    {
        // Expired a moment ago; an in-progress join inside the grace window still completes.
        var expiresAt = Now - TimeSpan.FromMinutes(1);
        InviteJoinRules.IsJoinable(InviteStatus.Active, expiresAt, Now).Should().BeTrue();
    }

    [Fact]
    public void Active_AtExactGraceBoundary_IsJoinable()
    {
        var expiresAt = Now - InviteJoinRules.JoinGracePeriod; // now == expiresAt + grace
        InviteJoinRules.IsJoinable(InviteStatus.Active, expiresAt, Now).Should().BeTrue();
    }

    [Fact]
    public void Active_BeyondGrace_IsNotJoinable()
    {
        var expiresAt = Now - InviteJoinRules.JoinGracePeriod - TimeSpan.FromSeconds(1);
        InviteJoinRules.IsJoinable(InviteStatus.Active, expiresAt, Now).Should().BeFalse();
    }

    [Fact]
    public void Used_IsNeverJoinable_EvenWithinWindow()
    {
        // Single-use: a consumed invite never re-opens, regardless of the clock.
        InviteJoinRules.IsJoinable(InviteStatus.Used, Now.AddDays(3), Now).Should().BeFalse();
    }

    [Fact]
    public void Expired_IsNotJoinable()
    {
        InviteJoinRules.IsJoinable(InviteStatus.Expired, Now.AddDays(3), Now).Should().BeFalse();
    }
}
