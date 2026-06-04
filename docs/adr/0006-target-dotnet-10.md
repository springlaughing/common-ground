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
  `--configuration Release`, as SAC can block unsigned Debug build outputs. **Smart App Control (SAC)** is a Windows 11 security feature (22H2+) that blocks executables it doesn't trust. Unlike SmartScreen, it's aggressive: it allows apps that are either signed by a known publisher or have an established cloud reputation, and blocks anything unsigned/unknown outright. (It has three modes — Evaluation / On / Off — and once turned Off it can only be re-enabled by reinstalling Windows.)

    - Why it bites EF migrations: dotnet ef database update doesn't just compile — it has to run your app's design-time host to load the DbContext. To do that, .NET produces a small native launcher (the apphost, CommonGround.Api.exe) and executes it. That freshly-built apphost is unsigned and has zero reputation, so SAC kills it → the migration fails with a vague access/blocked error rather than an EF error, which makes it confusing.

    - Why --configuration Release was the workaround: it's an empirical fix — building/running through the Release output path avoided the SAC block in practice (different output binaries, and the migration flow ends up running an already-validated Release build rather than re-launching a just-built Debug apphost). 
- *(Resolved 2026-06-04)* `quickstart.md` referenced .NET 9 and has been updated to
  .NET 10; `spec.md` does not pin a framework version (the tech context lives in
  `plan.md`), so no change was needed there.

## Alternatives considered

### Stay on .NET 9 (as originally planned)

Rejected. .NET 9 is STS (18-month support). Its tooling (including Stryker) works today,
but the shorter support window is a poor fit for a project intended to remain credible
long-term. The Stryker gap on .NET 10 is temporary; the STS support-window disadvantage
is structural.

## Future options

- Promote Stryker (`mutation.yml`) to a required PR check once a .NET 10-compatible
  release ships — bump the version in `backend/.config/dotnet-tools.json`.
