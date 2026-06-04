# ADR 0005: Use GitHub Actions for CI/CD

## Status

Accepted — *Decision* reconciled with the as-built pipeline on 2026-06-04
(implemented items are separated from planned ones).

## Context

CommonGround's constitution requires an audit-ready DevSecOps pipeline. Every PR must
produce verifiable evidence: test reports, coverage, quality gate results, security
scans, and build artifacts. The project is hosted on GitHub.

The CI/CD platform must support the full evidence chain:

- Build and test on every PR
- Code coverage collection and reporting
- Static analysis and quality gate enforcement
- Dependency vulnerability scanning
- Secret scanning
- SBOM generation on release
- Build artifact upload
- Artifact provenance and attestation
- Deployment records

## Decision

Use **GitHub Actions** as the CI/CD platform.

### Workflow structure

Workflows under `.github/workflows/`. The descriptions below reflect what is
**implemented as of 2026-06-04**; items marked *(planned)* are intended but not yet
built.

**`ci.yml`** — runs on push to `main` / `001-*` / `feature/*` and PRs to `main`:
- Backend job *"Backend (.NET 10)"*: restore → build (`--configuration Release`) →
  `dotnet test` with `XPlat Code Coverage` collection, against `CommonGround.slnx`
- Frontend job *"Frontend (React)"*: `npm ci` → type-check → ESLint → Vitest → build
- *(planned)* publish `.trx` and coverage as artifacts; split unit / integration /
  architecture into separate reported steps

**`sonar.yml`** — SonarCloud quality gate via the MSBuild-integrated .NET scanner; one
analysis covering C# (dotnet-coverage `vscoveragexml`) + TypeScript (Vitest lcov). Runs
on push/PR to `main` **only** — the SonarCloud free tier analyzes just `main` + PRs.
Requires the `SONAR_TOKEN` secret and "Automatic Analysis" disabled.

**`security.yml`** — CodeQL analysis for C# and JS/TS, on push/PR to `main` and a weekly
(Monday) schedule.
- *(planned)* Dependency Review action. Secret scanning is a repository-level setting,
  not a workflow.

**`release.yml`** — on `v*` tags: build + `dotnet publish` the backend and create a
GitHub release with auto-generated notes.
- *(planned)* SBOM in CycloneDX format, build-artifact upload as a release asset, and
  GitHub artifact attestation linking the build to repo + commit SHA + workflow run +
  environment. *(These are the provenance pieces relevant to CRA/SLSA — see "Future
  options".)*

**`mutation.yml`** — Stryker.NET mutation testing for the deterministic scoring engine
(target ≥ 60%, the constitution's critical-business-logic gate). Currently **manual
(`workflow_dispatch`) and blocked**: Stryker 4.14.2 cannot design-time-build .NET 10
projects or read `.slnx`. Config (`backend/stryker-config.json`), a classic
`backend/Stryker.sln`, and the pinned tool manifest are committed and ready; promote to
a required PR check once a .NET 10-compatible Stryker ships.

### Supporting configuration

- **`dependabot.yml`** — Dependabot for NuGet (.NET) and npm (frontend), weekly schedule
- Branch protection on `main`: PR required; required checks = *"Backend (.NET 10)"* +
  *"Frontend (React)"*; conversation resolution required; force-push and deletion
  disabled; **0 required approvals** (solo project — review is self-review)
- *(planned)* `CODEOWNERS`; a `deploy.yml` deployment workflow with GitHub Environment
  approval and deployment records

### Quality gate thresholds (SonarCloud)

- No new critical bugs
- No new critical vulnerabilities
- Coverage on new code ≥ 70% (target 80%)
- Duplicated lines on new code ≤ 3%
- Maintainability and security ratings acceptable

## Consequences

### Positive

- GitHub Actions is natively integrated — no separate CI service to configure or
  authenticate.
- Free for public repositories; generous free minutes for private repositories.
- The implemented evidence chain — build, tests, coverage collection, SonarCloud quality
  gate, and CodeQL — runs on every PR to `main`. The remaining provenance pieces (SBOM,
  artifact attestation, deployment records) are planned (see Decision) to complete it.
- Dependabot and CodeQL are GitHub-native — no additional tools or accounts needed
  for dependency and security scanning.
- GitHub Environments provide deployment approval records out of the box.
- Workflow files are version-controlled in the repository — configuration is evidence.

### Negative

- GitHub Actions has vendor lock-in. Migrating to another CI platform would require
  rewriting workflows.
- Free tier has concurrent job limits; large test suites may queue on busy periods.

## Alternatives considered

### Azure DevOps Pipelines

Strong .NET support and deep Microsoft integration. Rejected because it requires a
separate Azure DevOps organisation and adds account management overhead. GitHub
Actions covers all required capabilities without leaving the GitHub ecosystem.

### Jenkins

Highly flexible and self-hosted. Rejected because it requires infrastructure to run
and maintain the Jenkins server, which adds operational cost and complexity
disproportionate to an MVP.

### CircleCI / BuildKite

Capable platforms with good .NET support. Rejected because they require separate
accounts and configuration outside GitHub. GitHub Actions provides equivalent
functionality with less friction for a GitHub-hosted project.

## Future options

- Self-hosted GitHub Actions runners if build times or costs become an issue.
- Reusable workflow files (`workflow_call`) if the project grows to multiple
  repositories.
- GitHub Advanced Security for additional secret scanning and dependency review
  features if the project moves to a private repository.
