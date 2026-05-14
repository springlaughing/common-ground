# ADR 0005: Use GitHub Actions for CI/CD

## Status

Accepted

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

Four workflow files under `.github/workflows/`:

**`ci.yml`** — runs on every PR and push to main:
- Build solution
- Run unit tests (xUnit) and publish `.trx` results
- Run integration tests (xUnit + Testcontainers) and publish results
- Run architecture tests
- Collect coverage with Coverlet and publish HTML/XML artifacts
- Send coverage to SonarCloud
- Evaluate SonarCloud quality gate — PR fails if gate does not pass
- Run ESLint and TypeScript compiler check on frontend
- Run frontend tests (React Testing Library, Playwright)

**`security.yml`** — runs on every PR and on a nightly schedule:
- CodeQL analysis for C# and TypeScript
- Dependency review (GitHub Dependency Review Action)
- Secret scanning is enabled at the repository level via GitHub Advanced Security

**`release.yml`** — runs on version tags:
- Full build and test suite
- Generate SBOM in CycloneDX format (`sbom.cdx.json`)
- Upload build artifact and SBOM as release assets
- Create GitHub artifact attestation linking the artifact to repo, commit SHA,
  workflow run, and build environment
- Publish release notes

**`deploy.yml`** — runs after a successful release or manually:
- Deploy to target environment
- Record commit SHA, artifact version, timestamp, and environment
- Run smoke tests against the deployed instance
- Require manual approval for production via GitHub Environment protection rules

### Supporting configuration

- **`dependabot.yml`** — Dependabot configured for NuGet (.NET) and npm (frontend),
  weekly update schedule
- **`CODEOWNERS`** — defines who must review changes to critical paths
- Branch protection on `main`: require PR, require CI pass, require review

### Quality gate thresholds (SonarCloud)

- No new critical bugs
- No new critical vulnerabilities
- Coverage on new code ≥ 70% (target 80%)
- Duplicated lines on new code ≤ 3%
- Maintainability and security ratings acceptable

### Mutation testing (Stryker.NET)

Mutation testing is slow and is not run on every PR. It runs:
- On the `main` branch after merge
- Nightly via scheduled workflow
- Manually before a release

Results are published as workflow artifacts.

## Consequences

### Positive

- GitHub Actions is natively integrated — no separate CI service to configure or
  authenticate.
- Free for public repositories; generous free minutes for private repositories.
- The full evidence chain (test reports, coverage, quality gate, SBOM, attestation,
  deployment record) is produced and stored as GitHub artifacts and release assets.
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
