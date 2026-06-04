# ADR 0006: Target .NET 10 (LTS) instead of .NET 9

## Status

Accepted

## Context

The original spec, plan, and quickstart specified **.NET 9**. During project setup
(T001/T002) the backend was built on **.NET 10** (SDK 10.0.102) instead, and that choice
has carried through all backend projects.

.NET follows a fixed release cadence: even-numbered releases (8, 10) are **LTS** with
~3 years of support; odd-numbered releases (7, 9) are **STS** with 18 months. .NET 9
(the originally-planned version) is STS; **.NET 10 (GA November 2025) is the current
LTS.**

This is a significant, spec-deviating technical decision and therefore warrants an ADR
(constitution §VII).

## Decision

Target **.NET 10 (LTS)** across all backend projects (target framework `net10.0`; SDK
pinned in `global.json`). CI, CodeQL, the SonarCloud scan, and the release build all use
`setup-dotnet` `10.0.x`.

**Rationale:** .NET 10 is the latest LTS at project start, giving the **longest support
window (~3 years vs .NET 9's 18-month STS)**. For a portfolio project meant to stay
buildable and credible over time, the longer-supported LTS is the better default, and
moving from an STS to an LTS baseline is a net improvement over the original plan.

## Consequences

### Positive

- Longest support runway — security and servicing patches for ~3 years.
- Access to C# 14, the latest ASP.NET Core, and EF Core 10.
- One SDK aligns local dev, CI, CodeQL, Sonar, and release builds.

### Negative / known trade-offs

- **Stryker.NET mutation testing is blocked.** Stryker 4.14.2 (incl. the prerelease
  channel) cannot design-time-build .NET 10 projects (bundled Buildalyzer returns 0
  analyzable projects) and cannot read the `.slnx` solution format. Mutation testing
  (constitution §VII, critical-logic gate) is staged but cannot run until a compatible
  Stryker ships. See [ADR-0005](0005-use-github-actions-for-ci-cd.md) and `mutation.yml`.
- Being on the newest LTS means some ecosystem tooling lags briefly (the Stryker gap
  above is the concrete instance).
- **Smart App Control (Windows) workaround:** local EF migrations must run with
  `--configuration Release`, as SAC can block unsigned Debug build outputs.
- The spec and quickstart still reference .NET 9 and should be updated for consistency.

## Alternatives considered

### Stay on .NET 9 (as originally planned)

Rejected. .NET 9 is STS (18-month support). Its tooling (including Stryker) works today,
but the shorter support window is a poor fit for a project intended to remain credible
long-term. The Stryker gap on .NET 10 is temporary; the STS support-window disadvantage
is structural.

## Future options

- Promote Stryker (`mutation.yml`) to a required PR check once a .NET 10-compatible
  release ships — bump the version in `backend/.config/dotnet-tools.json`.
- Update `spec.md` and `quickstart.md` to .NET 10.
